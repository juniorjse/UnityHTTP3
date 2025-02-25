using System.IO;
using System;
using System.Collections.Generic;
using Wildlife.PitayaCSharp.Error;
using Wildlife.PitayaCSharp.Util;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Client;
using UnityEngine.UI;

namespace Wildlife.PitayaCSharp.Transport
{
    public class PacketParser
    {
        Action<Packet> _onPacket; // Maybe it should be in the Transporter, thus making Parse return a Packet instead of calling onPacket handler already
        byte[] _messageHeader;
        int _messageHeaderOffset;
        byte[] _messageBody;
        int _messageBodyOffset;
        int _messageBodyLength;
        PacketParserState _state;

        public PacketParser(Action<Packet> onPacketHandler)
        {
            _onPacket = onPacketHandler;
            _messageHeader = new byte[PacketProtocol.HeaderLength];
            _messageHeaderOffset = 0;
            _messageBodyOffset = 0;
            _messageBodyLength = 0;
            _state = PacketParserState.ParseHead;
        }

        private void Reset()
        {
            _messageBody = null;
            _messageHeaderOffset = 0;
            _messageBodyOffset = 0;
            _messageBodyLength = 0;
            _state = PacketParserState.ParseHead;
        }

        internal void Parse(byte[] bytes, int offset, int limit)
        {
            if (offset < limit)
            {
                if (_state == PacketParserState.ParseHead)
                {
                    ParseHead(bytes, offset, limit);
                }
                else if (_state == PacketParserState.ParseBody)
                {
                    ParseBody(bytes, offset, limit);
                }
            }
            else if (_state == PacketParserState.ParseBody && _messageBodyLength == 0)
            { // there's no more content to parse, but this is a packet with an empty body
                ParseBody(bytes, offset, limit);
            }
        }

        private void ParseHead(byte[] bytes, int offset, int limit)
        {
            int needLength = PacketProtocol.HeaderLength - _messageHeaderOffset;
            int dataLength = limit - offset;
            int length = Math.Min(needLength, dataLength);

            Array.Copy(bytes, offset, _messageHeader, _messageHeaderOffset, length);
            _messageHeaderOffset += length;

            /* a complete head got */
            if (_messageHeaderOffset == PacketProtocol.HeaderLength)
            {
                PacketProtocol.ValidateHeaderInformation(_messageHeader);
                _messageBodyLength = PacketProtocol.GetPacketLengthFromHeader(_messageHeader);

                _messageBody = new byte[_messageBodyLength];
                _state = PacketParserState.ParseBody;
                _messageBodyOffset = 0;
            }

            Parse(bytes, offset + length, limit);
        }

        private void ParseBody(byte[] bytes, int offset, int limit)
        {
            int needLength = _messageBodyLength - _messageBodyOffset;
            int dataLength = limit - offset;
            int length = Math.Min(needLength, dataLength);

            Array.Copy(bytes, offset, _messageBody, _messageBodyOffset, length);
            _messageBodyOffset += length;

            if (_messageBodyOffset == _messageBodyLength)
            {
                /* a complete package parsed */
                //Invoke the protocol api to handle the message
                Packet packet = PacketProtocol.Parse(_messageHeader, _messageBody);
                _onPacket.Invoke(packet);
                Reset();
            }

            Parse(bytes, offset + length, limit);
        }

    }
}