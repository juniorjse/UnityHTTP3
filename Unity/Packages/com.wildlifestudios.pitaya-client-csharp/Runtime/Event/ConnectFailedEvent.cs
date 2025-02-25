using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Event
{
    public class ConnectFailedEvent : INetworkEvent
    {
        public string Arg1 { get; } // reason description
        public string Arg2 { get; }
        public ConnectFailedEvent(string reason, string arg2 = null)
        {
            Arg1 = reason;
            Arg2 = arg2;
        }

        public PitayaClientState GetNextClientState() { return PitayaClientState.Inited; }

        public PitayaNetWorkState GetNextNetworkState() { return PitayaNetWorkState.FailToConnect; }

        public override string ToString() { return "PC_EV_CONNECT_FAILED"; }
    }
}