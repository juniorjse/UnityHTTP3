using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Event
{
    public class ReconnectStartedEvent : IClientEvent
    {
        public string Arg1 { get; }
        public string Arg2 { get; }
        public ReconnectStartedEvent(string arg1 = null, string arg2 = null)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public PitayaClientState GetNextClientState() { return PitayaClientState.Connecting; }

        public override string ToString() { return "PC_EV_RECONNECT_STARTED"; }
    }
}