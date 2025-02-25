using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using Wildlife.PitayaCSharp.Error;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Protobuf;
using Wildlife.PitayaCSharp.Compression;
using Wildlife.PitayaCSharp.SimpleJson;

namespace Wildlife.PitayaCSharp.Protocol
{
    public class MessageProtocol
    {
        readonly ConcurrentDictionary<string, ushort> _route2code = new ConcurrentDictionary<string, ushort>();
        readonly ConcurrentDictionary<ushort, string> _code2route = new ConcurrentDictionary<ushort, string>();
        readonly ConcurrentDictionary<uint, string> _reqMap = new ConcurrentDictionary<uint, string>();
        public const byte ErrorMask = 0x20;
        public const byte GzipMask = 0x10;
        public const byte MessageHeadLength = 0x02;
        public const byte MessageRouteLengthMask = 0xFF;
        public const byte MessageRouteCompressMask = 0x01;
        public const byte MessageTypeMask = 0x07;
        public const byte MessageFlagBytes = 0x01;

        public MessageProtocol(Dictionary<string, ushort> dict)
        {
            foreach (string key in dict.Keys)
            {
                ushort code = dict[key];
                string route = key.Trim();

                // duplication check
                if (_route2code.ContainsKey(route) || _code2route.ContainsKey(code))
                {
                    throw new Exception($"Duplicated route (route: {route}, code: {code})");
                }

                // update map, using last value when key duplicated
                _route2code.TryAdd(route, code);
                _code2route.TryAdd(code, route);
            }
        }

        public byte[] Encode(Message message, bool compressData)
        {
            int routeLength = ByteLength(message.Route);
            if (routeLength > MessageRouteLengthMask)
            {
                throw new ErrRouteLengthExceeded();
            }

            //initializing result buf
            var buf = new List<byte>();

            //body
            int wasBodyCompressed = 0;


            message.Data ??= new byte[0];
            
            byte[] body = (compressData && message.Data.Length > 0)
                    ? EncodeMessageBody(message.Data, out wasBodyCompressed) // encode it using Gzip
                    : message.Data;

            // head
            // encode head
            MessageType type = (message.Id == 0) ? MessageType.Notify : MessageType.Request;

            ushort routeCode;
            bool isRouteCompressed = false;
            if (_route2code.TryGetValue(message.Route, out routeCode))
            {
                if (routeCode > 0)
                    isRouteCompressed = true;
            }

            /* flag */
            buf.Add(EncodeMessageFlag(type, isRouteCompressed, wasBodyCompressed));

            /* message id */
            if (Identifiable(type))
            {
                buf.AddRange(EncodeMessageId(message.Id));
                _reqMap.TryAdd(message.Id, message.Route); // Add id to route map
            }

            /* route */
            if (Routable(type))
            {
                if (isRouteCompressed)
                {
                    buf.Add((byte)((routeCode >> 8) & 0xFF));
                    buf.Add((byte)(routeCode & 0xFF));
                }
                else
                {
                    buf.Add((byte)(routeLength & 0xFF));
                    buf.AddRange(System.Text.Encoding.UTF8.GetBytes(message.Route));
                }
            }

            /* body */
            buf.AddRange(body);

            byte[] result = buf.ToArray();

            PitayaClientLib.Logger.LogDebug("pc_default_msg_encoder - buf encoded with length " + result.Length);

            return result;
        }

        public Message Decode(byte[] data)
        {
            RawMessage rawMessage = DecodeToRaw(data);

            Message message = new Message(rawMessage);

            if (Routable(rawMessage.Type))
            {
                if (rawMessage.IsRouteCompressed)
                {
                    if (_code2route.TryGetValue(rawMessage.Route.RouteCode, out string route))
                    {
                        message.Route = route;
                    }
                    else
                    {
                        PitayaClientLib.Logger.LogError("pc_default_msg_decode - fail to uncompress route dictionary: " + rawMessage.Route.RouteCode);
                        throw new ErrRouteInfoNotFound();
                    }
                }
            }

            if (Routable(rawMessage.Type) && message.Route == null)
            {
                throw new ErrPushMessageWithoutRoute();
            }

            if (rawMessage.IsGzipped && rawMessage.Body.Length > 0)
            {
                try
                {
                    message.Data = Gzip.InflateData(rawMessage.Body);

                    PitayaClientLib.Logger.LogDebug(string.Format("pc_default_msg_decode decompressed msg: {0} -> {1} bytes", rawMessage.Body.Length, message.Data.Length));
                }
                catch (Exception)
                {
                    PitayaClientLib.Logger.LogError("pc_default_msg_decode - gzip inflate error");
                    throw;
                }
            }

            return message;
        }

        private RawMessage DecodeToRaw(byte[] data)
        {
            int len = data.Length;
            if (len < MessageFlagBytes)
                OnMessageLengthError();

            //Decode head
            //Get flag            
            byte flag = data[0];

            //Set offset to 1, for the 1st byte will always be the flag
            int offset = 1;

            //Get type from flag;
            MessageType messageType = (MessageType)((flag >> 1) & MessageTypeMask);

            if (InvalidType(messageType))
            {
                PitayaClientLib.Logger.LogError("pc_msg_decode_to_raw - unknow message type");
                throw new ErrWrongMessageType();
            }

            uint id = 0;
            string messageRouteString;
            if (Identifiable(messageType))
            {
                int length;
                id = Decoder.DecodeUInt32(offset, data, out length);
                if (id <= 0 || !_reqMap.TryRemove(id, out messageRouteString))
                {
                    throw new Exception("Not yet implemented!!!");
                }
                offset = length;
            }

            bool isRouteCompressed = (flag & MessageRouteCompressMask) == MessageRouteCompressMask;

            // route
            MessageRoute route = new MessageRoute();
            if (Routable(messageType))
            {
                if (isRouteCompressed)
                {
                    if ((offset + 2) > len)
                        OnMessageLengthError();

                    // The line below should be the same as: code := binary.BigEndian.Uint16(data[offset:(offset + 2)])
                    route.RouteCode = ReadShort(offset, data);
                    offset += 2;
                }
                else
                {
                    byte routeLength = data[offset];
                    offset++;

                    if (offset > len || (offset + routeLength) > len)
                        OnMessageLengthError();

                    route.RouteString = System.Text.Encoding.UTF8.GetString(data, offset, routeLength);
                    offset += routeLength;
                }
            }

            bool isGzipped = (flag & GzipMask) == GzipMask;
            bool error = (flag & ErrorMask) == ErrorMask;
            byte[] body = new byte[data.Length - offset];
            Array.Copy(data, offset, body, 0, body.Length);

            RawMessage message = new RawMessage(id, messageType, isRouteCompressed, isGzipped, error, route, body);

            return message;
        }

        private ushort ReadShort(int offset, byte[] bytes)
        {
            return (ushort)((bytes[offset] << 8) + bytes[offset + 1]);
        }

        private int ByteLength(string message)
        {
            return System.Text.Encoding.UTF8.GetBytes(message).Length;
        }

        private bool InvalidType(MessageType type)
        {
            return type < MessageType.Request || type > MessageType.Push;
        }

        private bool Routable(MessageType type)
        {
            return type != MessageType.Response;
        }

        private bool Identifiable(MessageType type)
        {
            return type == MessageType.Request || type == MessageType.Response;
        }

        private void OnMessageLengthError()
        {
            PitayaClientLib.Logger.LogError("pc_msg_decode_to_raw - invalid length");
            throw new ErrInvalidMessage();
        }

        private byte EncodeMessageFlag(MessageType messageType, bool isRouteCompressed, int wasBodyCompressed)
        {
            byte flag = (byte)(((byte)messageType) << 1); // encode message type in flag
            flag |= (byte)((isRouteCompressed) ? MessageRouteCompressMask : 0); // encode compressed route info (0x1)
            flag |= (byte)(wasBodyCompressed << 4); // encode body compressed info (0x20)

            return flag;
        }

        private byte[] EncodeMessageId(uint id)
        {
            return Encoder.EncodeUInt32(id);
        }

        private byte[] EncodeMessageBody(byte[] body, out int wasBodyCompressed)
        {
            byte[] result = body;
            wasBodyCompressed = 0;

            try
            {
                byte[] compressedBody = Gzip.DeflateData(body);
                if (compressedBody.Length >= body.Length)
                {
                    PitayaClientLib.Logger.LogDebug(string.Format("pc_body_json_encode - compressed is larger ({0} > {1})", compressedBody.Length, body.Length));
                }
                else
                {
                    result = compressedBody;
                    wasBodyCompressed = 1;
                }
            }
            catch (Exception)
            {
                PitayaClientLib.Logger.LogError("pc_body_json_encode - error compressing data");
            }

            return result;
        }
    }
}