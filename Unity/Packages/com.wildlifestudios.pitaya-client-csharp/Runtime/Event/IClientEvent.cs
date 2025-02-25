using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Event
{
    /* arg1 and arg2 are significant for the following events:
    *   PC_EV_USER_DEFINED_PUSH - arg1 as push route, arg2 as push msg
    *   PC_EV_CONNECT_ERROR - arg1 as short error description
    *   PC_EV_CONNECT_FAILED - arg1 as short reason description
    *   PC_EV_UNEXPECTED_DISCONNECT - arg1 as short reason description
    *   PC_EV_PROTO_ERROR - arg1 as short reason description
    *   PC_EV_RECONNECT_FAILED - arg1 as short reason description
    *
    * For other events, arg1 and arg2 will be set to NULL.
    */
    public interface IClientEvent : IPitayaEvent
    {
        public PitayaClientState GetNextClientState();
    }
}