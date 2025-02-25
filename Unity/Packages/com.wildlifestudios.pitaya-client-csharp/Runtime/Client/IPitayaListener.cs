using System;
using Wildlife.PitayaCSharp.Error;

namespace Wildlife.PitayaCSharp.Client
{
    public interface IPitayaListener
    {
        void OnRequestResponse(uint rid, byte[] body);
        void OnRequestError(uint rid, PitayaError error);
        void OnNetworkEvent(PitayaNetWorkState state, NetworkError error);
        void OnUserDefinedPush(string route, byte[] serializedBody);
    }
}