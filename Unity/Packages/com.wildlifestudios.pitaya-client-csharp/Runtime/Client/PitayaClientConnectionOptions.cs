using System;

namespace Wildlife.PitayaCSharp.Client
{
    public class PitayaClientConnectionOptions
    {
        public System.Net.Sockets.Socket Socket { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string HandshakeOpts { get; set; }

        public PitayaClientConnectionOptions(System.Net.Sockets.Socket socket = null, string host = null, int port = default, string handshakeOpts = null)
        {
            Socket = socket;
            Host = host;
            Port = port;
            HandshakeOpts = handshakeOpts;
        }

        public void Deconstruct(out System.Net.Sockets.Socket client, out string host, out int port, out string handshakeOpts)
        {
            client = Socket;
            host = Host;
            port = Port;
            handshakeOpts = HandshakeOpts;
        }
    }
}