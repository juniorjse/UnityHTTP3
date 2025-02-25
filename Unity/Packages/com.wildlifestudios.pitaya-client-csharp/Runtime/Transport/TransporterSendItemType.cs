namespace Wildlife.PitayaCSharp.Transport
{
    public enum TransporterSendItemType
    {
        None = 0x10,
        Notify = 0x20,
        Response = 0x40,
        Internal = 0x80 // handshake and heartbeat
    }
}