using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Event
{
    public class UserDefinedPushEvent : IPitayaEvent
    {
        public string Arg1 { get; }
        public string Arg2 { get; }
        public UserDefinedPushEvent(string reason, string arg2)
        {
            Arg1 = reason;
            Arg2 = arg2;
        }

        public override string ToString() { return "PC_EV_USER_DEFINED_PUSH"; }
    }
}