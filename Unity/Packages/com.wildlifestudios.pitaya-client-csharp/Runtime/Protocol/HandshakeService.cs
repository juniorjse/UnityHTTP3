using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Wildlife.PitayaCSharp.Serializer;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Transport;

namespace Wildlife.PitayaCSharp.Protocol
{
    public class HandshakeService
    {
        ITransporter _transporter;
        Action<byte[]> _callback;

        public HandshakeService(ITransporter transporter)
        {
            _transporter = transporter;
        }

        public void Request(string handshakeOpts, Action<byte[]> callback)
        {
            if(handshakeOpts == "h3")
                handshakeOpts = null;

            byte[] body = BuildMessage(handshakeOpts);

            PitayaClientLib.Logger.LogDebug("tcp__send_handshake -- sending handshake: " + Encoding.UTF8.GetString(body));

            byte[] packet = PacketProtocol.Encode(PacketType.Handshake, body);

            _transporter.Send(packet, uint.MaxValue, uint.MaxValue, -1);

            _callback = callback;
        }

        internal void InvokeCallback(byte[] data)
        {
            //Invoke the handshake callback
            if (_callback != null)
            {
                _callback.Invoke(data);
                _callback = null;
            }
        }

        public void Ack()
        {
            PitayaClientLib.Logger.LogInfo("tcp__send_handshake_ack - send handshake ack");
            byte[] packet = PacketProtocol.Encode(PacketType.HandshakeAck, new byte[0]);

            _transporter.Send(packet, uint.MaxValue, uint.MaxValue, -1);
        }

        private byte[] BuildMessage(string handshakeOpts)
        {
            // build sys option;
            var sys = new HandshakeClientData
            {
                Platform = PitayaClientLib.Info.Platform,
                LibVersion = PitayaClientLib.GetVersionString(),
                BuildNumber = PitayaClientLib.Info.BuildNumber,
                Version = PitayaClientLib.Info.Version,
            };

            // build user option;
            Dictionary<string, object> user;

            if (string.IsNullOrEmpty(handshakeOpts))
            {
                user = new Dictionary<string, object>();
            }
            else
            {
                user = JsonConvert.DeserializeObject<Dictionary<string, object>>(handshakeOpts);
            }

            // build handshake message
            var message = new SessionHandshakeData
            {
                Sys = sys,
                User = user,
            };

            var messageJson = JsonConvert.SerializeObject(message);

            return Encoding.UTF8.GetBytes(messageJson);
        }
    }
}
