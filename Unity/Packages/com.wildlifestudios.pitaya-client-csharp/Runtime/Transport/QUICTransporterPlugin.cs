using System;
using System.IO;
using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Transport
{
    public class QUICTransporterPlugin : ITransporterPlugin
    {
        static readonly QUICTransporterPlugin _instance = new QUICTransporterPlugin();
        public static ITransporterPlugin Instance => _instance;
        private QUICTransporterPlugin() { }
        public TransporterName Name => TransporterName.QUIC;
        public ITransporter CreateTransporter(IPitayaClient pitayaClient, IPitayaClientConfig clientConfig)
        {
            return new QUICTransporter(pitayaClient, clientConfig);
        }
        public void OnRegister() { }
        public void OnDeregister() { }
    }
}