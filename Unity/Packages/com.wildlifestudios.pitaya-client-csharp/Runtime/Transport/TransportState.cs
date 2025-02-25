namespace Wildlife.PitayaCSharp.Transport
{
    public enum TransportState
    {
        NotConnected = 0,
        Connecting = 1,
        Handshakeing = 2,
        Done = 3, // can receive and send data
    }
}
