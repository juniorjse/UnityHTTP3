using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Wildlife.PitayaCSharp.Error;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Serializer;
using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Util
{
    public static class Utils
    {
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

}