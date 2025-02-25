namespace Wildlife.PitayaCSharp.Protocol 
{
    public enum PacketType {
        Handshake = 1,
        HandshakeAck = 2,
        Heartbeat = 3,
        Data = 4,
        Kick = 5
    }
}