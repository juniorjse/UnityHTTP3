using Wildlife.PitayaCSharp.Serializer;
using Wildlife.PitayaCSharp.SimpleJson;
using System.Collections.Generic;

namespace Wildlife.PitayaCSharp.Protocol
{
    public class HandshakeSys
    {
        public Dictionary<string, ushort> Dict;
        public int Heartbeat;
        public ProtobufSerializer.SerializationFormat? Serializer;
        public bool UseGzip;
        public JsonObject Protos;
    }

    public class HandshakeData
    {
        public int Code;
        public HandshakeSys Sys;
    }
}