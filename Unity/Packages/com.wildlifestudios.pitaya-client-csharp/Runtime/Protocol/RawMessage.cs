namespace Wildlife.PitayaCSharp.Protocol
{
    public class RawMessage
    {
        public uint Id;
        public MessageType Type;
        public bool IsRouteCompressed;
        public bool IsGzipped;
        public bool Error;
        public MessageRoute Route;
        public byte[] Body;

        public RawMessage(uint id = default, MessageType type = default, bool isRouteCompressed = default, bool isGzipped = default, bool error = default, MessageRoute route = default, byte[] body = default)
        {
            Id = id;
            Type = type;
            IsRouteCompressed = isRouteCompressed;
            IsGzipped = isGzipped;
            Error = error;
            Route = route;
            Body = body;
        }
    }
}