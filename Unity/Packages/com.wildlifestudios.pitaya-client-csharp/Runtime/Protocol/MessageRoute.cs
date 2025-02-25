namespace Wildlife.PitayaCSharp.Protocol
{
    public class MessageRoute
    {
        public string RouteString;
        public ushort RouteCode;

        public MessageRoute(string routeString = default, ushort routeCode = default){
            RouteString = routeString;
            RouteCode = routeCode;
        }
    }
}