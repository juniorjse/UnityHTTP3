using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Transport
{
    public interface ITransporterPlugin
    {
        TransporterName Name { get; }
        ITransporter CreateTransporter(IPitayaClient pitayaClient, IPitayaClientConfig clientConfig);
        void OnRegister();
        void OnDeregister();
    }
}
