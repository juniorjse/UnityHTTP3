using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Transport
{
    public class TLSTransporterPlugin : ITransporterPlugin
    {
        static readonly TLSTransporterPlugin _instance = new TLSTransporterPlugin();
        public static ITransporterPlugin Instance => _instance;
        private TLSTransporterPlugin() { }
        public ITransporter CreateTransporter(IPitayaClient pitayaClient, IPitayaClientConfig clientConfig)
        {
            return new TCPTransporter(pitayaClient, clientConfig);
        }
        public void OnRegister()
        {
            TCPTransporterPlugin.Instance.OnRegister();
            _certificates = new X509Certificate2Collection();
            if (_certificates == null)
                PitayaClientLib.Logger.LogError("tr_uv_tls_plugin_on_register - tls error");
        }
        public void OnDeregister()
        {
            _certificates = null;
            TCPTransporterPlugin.Instance.OnDeregister();
        }
        public TransporterName Name => TransporterName.TLS;
        internal X509Certificate2Collection _certificates;

        public static void SetCAFile(string pathToCAFile)
        {
            try
            {
                var caCertificate = new X509Certificate2(pathToCAFile);
                _instance._certificates.Add(caCertificate);
            }
            catch (Exception exception)
            {
                PitayaClientLib.Logger.LogWarning("tr_uv_tls_set_ca_file - load verify locations error, cafile: " + pathToCAFile);
                throw new Exception("PC_RC_ERROR");
            }
        }
    }
}