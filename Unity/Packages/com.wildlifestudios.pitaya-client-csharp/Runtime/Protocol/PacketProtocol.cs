using System.IO;
using System;
using System.Collections.Generic;
using Wildlife.PitayaCSharp.Error;
using Wildlife.PitayaCSharp.Util;

namespace Wildlife.PitayaCSharp.Protocol
{
    /**
    * Pitaya package format:
    * +------+-------------+------------------+
    * | type | body length |       body       |
    * +------+-------------+------------------+
    *
    * Head: 4bytes
    *   0: package type, see as PacketType
    *   1 - 3: big-endian body length
    * Body: body length bytes
    */
    public class PacketProtocol
    {
        public const int MaxBodyBytes = 1 << 24; //16MB
        public const int PacketTypeBytes = 1; // amount of bytes in the header to store info about the type of the packet (see PacketType)
        public const int PacketBodyLengthBytes = 3; // amount of bytes in the header to store info about how long the body is
        public const int HeaderLength = PacketTypeBytes + PacketBodyLengthBytes;

        public static byte[] Encode(PacketType typ)
        {
            return Encode(typ, new byte[0]);
        }

        public static byte[] Encode(PacketType packetType, byte[] body)
        {
            if (packetType < PacketType.Handshake || packetType > PacketType.Kick)
            {
                throw new ErrWrongPomeloPacketType();
            }

            if (body.Length >= MaxBodyBytes)
            {
                throw new ErrPacketSizeExcced();
            }

            byte[] buf = new byte[HeaderLength + body.Length];

            buf[0] = Convert.ToByte(((byte)packetType) & 0xFF);

            int bodySize = body.Length;
            for (int i = HeaderLength - 1; i > 0; i--)
            {
                buf[i] = Convert.ToByte(bodySize & 0xFF);
                bodySize >>= 8;
            }

            Array.Copy(body, 0, buf, HeaderLength, body.Length);

            return buf;
        }

        internal static void ValidateHeaderInformation(byte[] header)
        {
            if (header.Length != HeaderLength)
            {
                throw new ErrInvalidPomeloHeader();
            }

            PacketType packetType = GetPacketTypeFromHeader(header);
            if (packetType < PacketType.Handshake || packetType > PacketType.Kick)
            {
                throw new ErrWrongPomeloPacketType();
            }

            int size = GetPacketLengthFromHeader(header);

            if (size > MaxBodyBytes)
            {
                throw new ErrPacketSizeExcced();
            }
        }

        internal static int GetPacketLengthFromHeader(byte[] header)
        {
            int packetLength = 0;
            /* skip the first byte which is the type */
            for (int i = 1; i < HeaderLength; i++)
            {
                packetLength <<= 8;
                packetLength += header[i] & 0xFF;
            }
            return packetLength;
        }

        internal static PacketType GetPacketTypeFromHeader(byte[] header)
        {
            return (PacketType)(header[0] & 0xFF);
        }

        internal static Packet Parse(byte[] header, byte[] body)
        {
            return new Packet(GetPacketTypeFromHeader(header), body);
        }
    }
}