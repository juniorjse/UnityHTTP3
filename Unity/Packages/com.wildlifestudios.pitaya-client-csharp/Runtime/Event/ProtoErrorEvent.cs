using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Event
{
    public class ProtoErrorEvent : INetworkEvent
    {
        public string Arg1 { get; } // reason description
        public string Arg2 { get; }
        public ProtoErrorEvent(string reason, string arg2 = null)
        {
            Arg1 = reason;
            Arg2 = arg2;
        }

        public PitayaClientState GetNextClientState() { return PitayaClientState.Connecting; }

        public PitayaNetWorkState GetNextNetworkState() { return PitayaNetWorkState.Error; }

        public override string ToString() { return "PC_EV_PROTO_ERROR"; }
    }
}