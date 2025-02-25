using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Event
{
    public class KickedByServerEvent : INetworkEvent
    {
        public string Arg1 { get; }
        public string Arg2 { get; }
        public KickedByServerEvent(string arg1 = null, string arg2 = null)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public PitayaClientState GetNextClientState() { return PitayaClientState.Inited; }

        public PitayaNetWorkState GetNextNetworkState() { return PitayaNetWorkState.Kicked; }

        public override string ToString() { return "PC_EV_KICKED_BY_SERVER"; }
    }
}