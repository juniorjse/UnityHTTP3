using System;
using System.IO;
using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Transport
{
    public class TCPTransporterPlugin : ITransporterPlugin
    {
        static readonly TCPTransporterPlugin _instance = new TCPTransporterPlugin();
        public static ITransporterPlugin Instance => _instance;
        private TCPTransporterPlugin() { }
        public TransporterName Name => TransporterName.TCP;
        public ITransporter CreateTransporter(IPitayaClient pitayaClient, IPitayaClientConfig clientConfig)
        {
            return new TCPTransporter(pitayaClient, clientConfig);
        }
        public void OnRegister() { }
        public void OnDeregister() { }
    }
}