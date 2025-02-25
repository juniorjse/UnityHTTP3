namespace Wildlife.PitayaCSharp.Protocol 
{
    public class Packet {
        public PacketType Type;
        public int Length;
        public byte[] Data;
        public Packet(PacketType type, byte[] data) {
            Type = type;
            Length = data.Length;
            Data = data;
        }
    }
}