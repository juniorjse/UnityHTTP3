using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Event
{
    public class ReconnectFailedEvent : IClientEvent
    {
        public string Arg1 { get; } // reason description
        public string Arg2 { get; }
        public ReconnectFailedEvent(string reason, string arg2 = null)
        {
            Arg1 = reason;
            Arg2 = arg2;
        }

        public PitayaClientState GetNextClientState() { return PitayaClientState.Inited; }

        public override string ToString() { return "PC_EV_RECONNECT_FAILED"; }
    }
}