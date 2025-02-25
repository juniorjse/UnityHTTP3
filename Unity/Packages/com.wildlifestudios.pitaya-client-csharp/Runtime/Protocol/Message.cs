namespace Wildlife.PitayaCSharp.Protocol
{
    public class Message
    {
        public MessageType Type;
        public uint Id;
        public string Route;
        public byte[] Data;
        public bool Error;

        public Message(MessageType type = default, uint id = uint.MaxValue, string route = null, byte[] data = null, bool error = false)
        {
            Type = type;
            Id = id;
            Route = route;
            Data = data;
            Error = error;
        }

        public Message(RawMessage rawMessage)
        {
            Type = rawMessage.Type;
            Id = rawMessage.Id;
            Route = rawMessage.Route?.RouteString;
            Data = rawMessage.Body;
            Error = rawMessage.Error;
        }
    }
}