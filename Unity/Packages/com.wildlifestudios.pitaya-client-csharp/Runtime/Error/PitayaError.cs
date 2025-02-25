using System;
using System.Text;
using System.Collections.Generic;
using Protos;
using Wildlife.PitayaCSharp.SimpleJson;
using Wildlife.PitayaCSharp.Serializer;

namespace Wildlife.PitayaCSharp.Error
{
    [Serializable]
    public class ErrInvalidPomeloHeader : Exception
    {
        public ErrInvalidPomeloHeader() : base("invalid header") { }
        public ErrInvalidPomeloHeader(string message) : base(message) { }
        public ErrInvalidPomeloHeader(string message, Exception inner) : base(message, inner) { }

    }

    [Serializable]
    public class ErrWrongPomeloPacketType : Exception
    {
        public ErrWrongPomeloPacketType() : base("wrong packet type") { }
        public ErrWrongPomeloPacketType(string message) : base(message) { }
        public ErrWrongPomeloPacketType(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class ErrPacketSizeExcced : Exception
    {
        public ErrPacketSizeExcced() : base("codec: packet size exceed") { }
        public ErrPacketSizeExcced(string message) : base(message) { }
        public ErrPacketSizeExcced(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class ErrWrongMessageType : Exception
    {
        public ErrWrongMessageType() : base("wrong message type") { }
        public ErrWrongMessageType(string message) : base(message) { }
        public ErrWrongMessageType(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class ErrInvalidMessage : Exception
    {
        public ErrInvalidMessage() : base("invalid message") { }
        public ErrInvalidMessage(string message) : base(message) { }
        public ErrInvalidMessage(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class ErrRouteInfoNotFound : Exception
    {
        public ErrRouteInfoNotFound() : base("route info not found in dictionary") { }
        public ErrRouteInfoNotFound(string message) : base(message) { }
        public ErrRouteInfoNotFound(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class ErrPushMessageWithoutRoute : Exception
    {
        public ErrPushMessageWithoutRoute() : base("push message without route") { }
        public ErrPushMessageWithoutRoute(string message) : base(message) { }
        public ErrPushMessageWithoutRoute(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class ErrRouteLengthExceeded : Exception
    {
        public ErrRouteLengthExceeded() : base("route is too long") { }
        public ErrRouteLengthExceeded(string message) : base(message) { }
        public ErrRouteLengthExceeded(string message, Exception inner) : base(message, inner) { }
    }

    public class PitayaError : Exception
    {
        public string Code { get; private set; }
        public string Msg { get; private set; }
        public IDictionary<string, string> Metadata { get; private set; }

        public PitayaError(string code, string message, IDictionary<string, string> metadata = null) : base(message)
        {
            Msg = message;
            Code = code;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }

    public static class PitayaErrorFactory
    {
        public static PitayaError CreatePitayaError(PitayaInternalError internalError, ProtobufSerializer.SerializationFormat serializer)
        {
            var rawData = internalError.Buffer;

            if (serializer == ProtobufSerializer.SerializationFormat.Protobuf)
            {
                Protos.Error error = new ProtobufSerializer(serializer).Decode<Protos.Error>(rawData);
                return new PitayaError(error.Code, error.Msg, error.Metadata);
            }

            var jsonStr = Encoding.UTF8.GetString(rawData);
            var json = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, object>>(jsonStr);

            var code = (string)json["code"];
            var msg = (string)json["msg"];

            Dictionary<string, string> metadata;
            if (json.ContainsKey("metadata"))
            {
                metadata = (Dictionary<string, string>)SimpleJson.SimpleJson.CurrentJsonSerializerStrategy.DeserializeObject(json["metadata"],
                    typeof(Dictionary<string, string>), new Dictionary<string, string>());
            }
            else
            {
                metadata = new Dictionary<string, string>();
            }

            return new PitayaError(code, msg, metadata);
        }
    }
}