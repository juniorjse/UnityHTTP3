using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Serializer;
using Wildlife.PitayaCSharp.Compression;
using Wildlife.PitayaCSharp.Error;
using Wildlife.PitayaCSharp.Util;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Event;

namespace Wildlife.PitayaCSharp.Transport
{
    public class TCPTransporter : ITransporter
    {
        MessageProtocol _messageProtocol;
        public TransportState State { get; private set; }
        TCPTransporterReadService _reader;
        TCPTransporterSendService _sender;
        TransporterNotifyService _notifier;
        HandshakeService _handshakeService;
        HeartbeatService _heartbeatService;
        public IPitayaClient Client { get; private set; }
        public IPitayaClientConfig ClientConfig { get; }
        public ProtobufSerializer.SerializationFormat? Serializer { get; private set; }
        public int Quality { get; private set; }
        bool _isConnecting;
        UVTimer _connectionTimeout;
        UVTimer _reconnectionTimeout;
        uint _reconnectionRetries;
        internal string _certificateName;
        PitayaClientConnectionOptions _connectionOptions;
        public ITransporterPlugin Plugin { get; private set; }

        public TCPTransporter(IPitayaClient pitayaClient, IPitayaClientConfig config)
        {
            Client = pitayaClient;
            ClientConfig = config;
            _connectionOptions = new();
            _notifier = new();

            if (pitayaClient is IPitayaListener pitayaListener)
                _notifier.Subscribe(pitayaClient, pitayaListener);

            _reader = new TCPTransporterReadService(OnPacketHandler, this, _notifier);
            _reader._onReconnect = Reconnect;

            _handshakeService = new HandshakeService(this);
            _reconnectionRetries = 0;
            Plugin = TCPTransporterPlugin.Instance;
            State = TransportState.NotConnected;
        }

        private void Start(Stream stream, string handshakeOpts, Action<byte[]> callback)
        {
            /* tcp connected. */
            State = TransportState.Handshakeing;

            Quality = 0;
            _reader.Start(stream);
            _sender = new TCPTransporterSendService(1000, this, _notifier);
            _sender.Start(stream);

            PitayaClientLib.Logger.LogInfo("tcp__conn_done_cb - tcp connected, sending handshake");
            _handshakeService.Request(handshakeOpts, callback);
        }

        private void OnReconnect(object state)
        {
            _reconnectionTimeout.Stop();
            Connect(_connectionOptions.Host, _connectionOptions.Port, _connectionOptions.HandshakeOpts);
        }

        private void Reset()
        {
            _heartbeatService?.Stop();
            Quality = 0;
            Serializer = null;

            if (_connectionOptions.Socket != null)
            {
                if (State != TransportState.NotConnected)
                {
                    // If the state is something other than not connected, 
                    // we close the socket since it is potentially going to be called
                    // again in a reconnection.
                    _connectionOptions.Socket.Close();
                    _connectionOptions.Socket.Dispose();
                }
                _connectionOptions.Socket = null;
            }

            // Set internal state to not connected.
            State = TransportState.NotConnected;
        }

        private void Reconnect()
        {
            Reset();

            State = TransportState.Connecting;

            if (!ClientConfig.IsAutomaticReconnectionEnabled)
            {
                PitayaClientLib.Logger.LogWarning("tcp__reconn - trans want to reconn, but reconn is disabled");
                _reconnectionRetries = 0;
                State = TransportState.NotConnected;
                return;
            }

            var reconnectStartedEvent = new ReconnectStartedEvent("Started the reconnection");
            _notifier.FireEvent(Client, reconnectStartedEvent, ClientConfig.IsPollingEnabled);

            _reconnectionRetries++;

            if (ClientConfig.ReconnectionMaxRetries != -1 && ClientConfig.ReconnectionMaxRetries < _reconnectionRetries)
            {
                PitayaClientLib.Logger.LogWarning("tcp__reconn - reconn times exceeded");
                var reconnectFailedEvent = new ReconnectFailedEvent("Exceed Max Retry");
                _notifier.FireEvent(Client, reconnectFailedEvent, ClientConfig.IsPollingEnabled);

                _reconnectionRetries = 0;
                State = TransportState.NotConnected;
                return;
            }

            int reconnectionIntervalInMilliseconds = 2000;
            PitayaClientLib.Logger.LogDebug("tcp__reconn - reconnect, delay: " + reconnectionIntervalInMilliseconds);

            _reconnectionTimeout = new();
            _reconnectionTimeout.Start(OnReconnect, null, reconnectionIntervalInMilliseconds);
        }

        private Socket CreateTCPSocket()
        {
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            return socket;
        }

        public void Connect(string host, int port, string handshakeOpts)
        {
            State = TransportState.Connecting;

            _connectionOptions.Socket = CreateTCPSocket();
            _connectionTimeout = new();

            _connectionOptions.Host = host;
            _connectionOptions.Port = port;
            _connectionOptions.HandshakeOpts = handshakeOpts;

            Dns.BeginGetHostAddresses(host, OnHostAddressesResolved, null);
        }

        private void OnHostAddressesResolved(IAsyncResult dnsResult)
        {
            try
            {
                var addresses = Dns.EndGetHostAddresses(dnsResult);

                if (addresses == null || addresses.Length == 0)
                {
                    var connectErrorEvent = new ConnectErrorEvent("DNS Resolve Error");
                    _notifier.FireEvent(Client, connectErrorEvent, ClientConfig.IsPollingEnabled);

                    PitayaClientLib.Logger.LogError(string.Format("tcp__conn_async_cb - dns resolve error: {0}, will reconn", _connectionOptions.Host));
                    Reconnect();
                    return;
                }

                if (ClientConfig.ConnectionTimeoutInMilliseconds != -1)
                {
                    PitayaClientLib.Logger.LogDebug("tcp__con_async_cb - start conn timeout timer");
                    _connectionTimeout.Start(OnConnectionTimeout, _connectionTimeout, ClientConfig.ConnectionTimeoutInMilliseconds);
                }

                _connectionOptions.Socket.BeginConnect(addresses[0], _connectionOptions.Port, OnConnectionDone, null);
                _isConnecting = true;
            }
            catch (Exception e)
            {
                PitayaClientLib.Logger.LogError("tcp__conn_async_cb - dns resolve error, state: " + Client.State);
                PitayaClientLib.Logger.LogError(string.Format("tcp__conn_async_cb - dns resolve error: {0}, will reconn", _connectionOptions.Host));
                var connectErrorEvent = new ConnectErrorEvent("DNS Resolve Error");
                _notifier.FireEvent(Client, connectErrorEvent, ClientConfig.IsPollingEnabled);
                Reconnect();
            }
        }

        private void OnConnectionTimeout(object? state)
        {
            var timer = (UVTimer)state;

            if (timer.WasStopped() || _isConnecting == false) return;

            PitayaClientLib.Logger.LogInfo("tcp__conn_timeout_cb - conn timeout, cancel it");
            _isConnecting = false;
            _connectionTimeout.Dispose();
        }

        private void OnConnectionDone(IAsyncResult tcpResult)
        {
            int handshakeTimeout = 0;

            if (!_isConnecting)
            {
                PitayaClientLib.Logger.LogDebug("tcp__conn_done_cb - connect timeout");
                var connectErrorEvent = new ConnectErrorEvent("Connect Timeout");
                _notifier.FireEvent(Client, connectErrorEvent, ClientConfig.IsPollingEnabled);
                Reconnect();
                return;
            }

            _isConnecting = false;

            if (ClientConfig.ConnectionTimeoutInMilliseconds != -1)
            {
                handshakeTimeout = (int)(ClientConfig.ConnectionTimeoutInMilliseconds - _connectionTimeout.ElapsedTimeAfterStart().TotalMilliseconds);
                _connectionTimeout.Stop();
                PitayaClientLib.Logger.LogDebug("handshake timeout: " + handshakeTimeout);
            }

            try
            {
                _connectionOptions.Socket.EndConnect(tcpResult);
            }
            catch (Exception e)
            {
                var connectErrorEvent = new ConnectErrorEvent("UV Conn Error", e.Message);
                _notifier.FireEvent(Client, connectErrorEvent, ClientConfig.IsPollingEnabled);

                PitayaClientLib.Logger.LogError(string.Format("tcp__conn_async_cb - uv tcp connect error: {0}, will reconn", e.Message));
                Reconnect();
                return;
            }

            Stream stream = new NetworkStream(_connectionOptions.Socket, true);

            if (ClientConfig.ClientTransporterName == TransporterName.TLS)
            {
                var sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(Utils.ValidateServerCertificate), null);

                PitayaClientLib.Logger.LogDebug(string.Format("Setting TLS SNI with hostname {0}.", _connectionOptions.Host));

                var plugin = (TLSTransporterPlugin)TLSTransporterPlugin.Instance;

                try
                {
                    sslStream.AuthenticateAsClient(_connectionOptions.Host, plugin._certificates, System.Security.Authentication.SslProtocols.Tls12, false);
                }
                catch (Exception exception)
                {
                    PitayaClientLib.Logger.LogError(string.Format("Error setting TLS SNI with hostname {0}. Error: {1}", _connectionOptions.Host, exception.InnerException.Message));
                    throw;
                }
                stream = sslStream;
            }

            try
            {
                Start(stream, _connectionOptions.HandshakeOpts, null);
            }
            catch (Exception)
            {
                throw new Exception("Cannot connect: invalid handshake options json data");
            }
        }

        private void OnHeartbeatResponse(DateTime hbLastSentAt, DateTime hbLastReceivedAt, int interval)
        {
            PitayaClientLib.Logger.LogDebug("tcp__on_heartbeat - [Heartbeat] received from server");

            TimeSpan span = hbLastReceivedAt - hbLastSentAt;
            int rtt = interval - (int)span.TotalMilliseconds;

            if (rtt >= 0)
                Quality = rtt;

            PitayaClientLib.Logger.LogDebug(string.Format("tcp__on_heartbeat - calc rtt: {0} ms", rtt));
        }

        //Send message, encode message
        public void Send(MessageType messageType, string route, uint sequenceNumber, byte[] data, uint requestUid, int timeout)
        {
            PitayaClientLib.Logger.LogDebug("tr_uv_tcp_send - ENTERED");

            if (State == TransportState.NotConnected)
                throw new PitayaInvalidStateException();

            byte[] body;

            try
            {
                Message message = new Message(messageType, requestUid, route, data);
                body = _messageProtocol.Encode(message, !ClientConfig.ShouldDisableCompression);
                PitayaClientLib.Logger.LogDebug("tr_uv_tcp_send - encoded msg length = " + body.Length);
            }
            catch (Exception)
            {
                PitayaClientLib.Logger.LogError("tr_uv_tcp_send - encode msg failed, route: " + route);
                throw new PitayaCommonErrorException();
            }

            Send(PacketType.Data, sequenceNumber, body, requestUid, timeout);
        }

        //Send message, encode packet
        private void Send(PacketType type, uint sequenceNumber, byte[] body, uint requestUid, int timeout)
        {
            byte[] packet;

            try
            {
                packet = PacketProtocol.Encode(type, body);
                PitayaClientLib.Logger.LogDebug("tr_uv_tcp_send - encoded package length = " + packet.Length);
            }
            catch (Exception)
            {
                PitayaClientLib.Logger.LogError("tr_uv_tcp_send - encode package failed");
                throw new PitayaCommonErrorException();
            }

            Send(packet, sequenceNumber, requestUid, timeout);
        }

        //Send message, use sender
        public void Send(byte[] packetBuffer, uint sequenceNumber, uint requestUid, int timeout)
        {
            // pre-alloc not implemented
            TransporterSendItem sendItem = new TransporterSendItem(packetBuffer, PitayaClient.DynamicAllocationFlag, sequenceNumber, requestUid, timeout);

            if (sendItem.Type != TransporterSendItemType.Internal)
            {
                PitayaClientLib.Logger.LogDebug(string.Format("tr_uv_tcp_send - use dynamic alloc write item, seq_num: {0}, req_id: {1}", sequenceNumber, requestUid));
                /* if not done, push it to connecting queue. */
                if (State == TransportState.Done)
                {
                    _sender.PutItemToWriteWaitQueue(sendItem);
                    PitayaClientLib.Logger.LogDebug(string.Format("tr_uv_tcp_send - put to write wait queue, seq_num: {0}, req_id: {1}", sendItem.SequenceNumber, sendItem.RequestUid));
                }
                else
                {
                    _sender.PutItemToConnectionPendingQueue(sendItem);
                    PitayaClientLib.Logger.LogDebug(string.Format("tr_uv_tcp_send - put to conn pending queue, seq_num: {0}, req_id: {1}", sendItem.SequenceNumber, sendItem.RequestUid));
                }

                PitayaClientLib.Logger.LogDebug(string.Format("tr_uv_tcp_send - seq num: {0}, req_id: {1}, length: {2}", sequenceNumber, requestUid, sendItem.Buffer.Length));
            }
            else
            {
                _sender.PutItemToWriteWaitQueue(sendItem);
            }

            if (State == TransportState.Connecting || State == TransportState.Handshakeing || State == TransportState.Done)
                _sender.Send();
        }

        //Invoke by Transporter, process the packet
        private void OnPacketHandler(Packet packet)
        {
            // Update the last packet that we received from the server,
            // in order to avoid a heartbeat timeout.
            PitayaClientLib.Logger.LogDebug("tr_tcp_on_pkg_handler - updating last server packet time");
            _heartbeatService?.ResetTimeout();

            //Ignore all the message except handshaking at handshake stage
            if (packet.Type == PacketType.Handshake && State == TransportState.Handshakeing)
            {
                OnHandshakeResponse(packet.Data);
            }
            else if (packet.Type == PacketType.Heartbeat && State == TransportState.Done)
            {
                _heartbeatService.InvokeCallback();
            }
            else if (packet.Type == PacketType.Data && State == TransportState.Done)
            {
                OnDataReceived(packet.Data);
            }
            else if (packet.Type == PacketType.Kick)
            {
                /*
                var bodyStr = Encoding.UTF8.GetString(packet.Data);
                JsonObject data = (JsonObject)SimpleJson.DeserializeObject(bodyStr);
                if (data == null || !data.ContainsKey("reason") || data["reason"] == null)
                {
                    data = new JsonObject();
                    data["reason"] = "kick";
                } */

                OnKickReceived();
                Close();
            }
        }

        private void OnHandshakeResponse(byte[] data)
        {
            HandshakeData? handshakeResponse = null;
            var serializer = Client.SerializerFactory.CreateJsonSerializer();

            if (Gzip.IsCompressed(data))
            {
                try
                {
                    data = Gzip.InflateData(data);
                    PitayaClientLib.Logger.LogInfo(string.Format("data: {0}.{1}", data.Length, Encoding.UTF8.GetString(data)));
                    handshakeResponse = serializer.Decode<HandshakeData>(data);
                }
                catch (Exception)
                {
                    PitayaClientLib.Logger.LogError("tcp__on_handshake_resp - failed to uncompress handshake data");
                }
            }
            else
            {
                PitayaClientLib.Logger.LogInfo(string.Format("data: {0}.{1}", data.Length, Encoding.UTF8.GetString(data)));
                handshakeResponse = serializer.Decode<HandshakeData>(data);
            }

            PitayaClientLib.Logger.LogInfo("tcp__on_handshake_resp - tcp get handshake resp");

            if (handshakeResponse == null)
            {
                PitayaClientLib.Logger.LogError("tcp__on_handshake_resp - handshake resp is not valid json");
                var connectFailedEvent = new ConnectFailedEvent("Handshake Error");
                _notifier.FireEvent(Client, connectFailedEvent, ClientConfig.IsPollingEnabled);
                Reset();
                return;
            }

            ProcessHandshakeData(handshakeResponse);
        }

        private void ProcessHandshakeData(HandshakeData handshakeData)
        {
            // Handshake code error
            if (handshakeData.Code != 200)
            {
                PitayaClientLib.Logger.LogError("tcp__on_handshake_resp - handshake fail, code: " + handshakeData.Code);
                var connectFailedEvent = new ConnectFailedEvent("Handshake Error");
                _notifier.FireEvent(Client, connectFailedEvent, ClientConfig.IsPollingEnabled);
                Reset();
                return;
            }

            // Handshake sys error
            if (handshakeData.Sys == null)
            {
                PitayaClientLib.Logger.LogError("tcp__on_handshake_resp - handshake fail, no sys field");
                var connectFailedEvent = new ConnectFailedEvent("Handshake Error");
                _notifier.FireEvent(Client, connectFailedEvent, ClientConfig.IsPollingEnabled);
                Reset();
                return;
            }

            PitayaClientLib.Logger.LogInfo("tcp__on_handshake_resp - handshake ok");

            //Set compress data
            var sys = handshakeData.Sys;

            var route2code = sys.Dict ?? new Dictionary<string, ushort>();

            _messageProtocol = new MessageProtocol(route2code);

            //Init heartbeat service

            int interval = sys.Heartbeat;

            if (interval <= 0)
            {
                // no need heartbeat
                interval = -1;
                PitayaClientLib.Logger.LogInfo("tcp__on_handshake_resp - no heartbeat specified");
            }
            else
            {
                PitayaClientLib.Logger.LogInfo("tcp__on_handshake_resp - set heartbeat interval: " + interval);
            }

            _heartbeatService = new HeartbeatService(interval, this, OnHeartbeatResponse);

            var sysSerializer = sys.Serializer;

            if (sysSerializer == null)
            {
                PitayaClientLib.Logger.LogWarning("tcp__on_handshake_resp - invalid serializer field sent by the server, defaulting to 'json'");
                sysSerializer = ProtobufSerializer.SerializationFormat.Json;
            }

            Serializer = sysSerializer.Value;

            //send ack and change protocol state
            _handshakeService.Ack();
            if (interval != -1)
            {
                PitayaClientLib.Logger.LogInfo("tcp__on_handshake_resp - start heartbeat interval timer");
                _heartbeatService.Start();
            }

            State = TransportState.Done;
            PitayaClientLib.Logger.LogInfo("tcp__on_handshake_resp - handshake completely");
            PitayaClientLib.Logger.LogInfo("tcp__on_handshake_resp - client connected");
            var connectedEvent = new ConnectedEvent();
            _notifier.FireEvent(Client, connectedEvent, ClientConfig.IsPollingEnabled);
        }

        private void OnDataReceived(byte[] data)
        {
            Message message;

            try
            {
                message = _messageProtocol.Decode(data);
            }
            catch (ErrPushMessageWithoutRoute)
            {
                PitayaClientLib.Logger.LogError("tcp__on_data_received - push message without route, error, will reconn");
                var protoErrorEvent = new ProtoErrorEvent("No Route Specified");
                _notifier.FireEvent(Client, protoErrorEvent, ClientConfig.IsPollingEnabled);
                Reconnect();
                return;
            }
            catch (Exception)
            {
                PitayaClientLib.Logger.LogError("tcp__on_data_received - decode error, will reconn");
                var protoErrorEvent = new ProtoErrorEvent("Decode Error");
                _notifier.FireEvent(Client, protoErrorEvent, ClientConfig.IsPollingEnabled);
                Reconnect();
                return;
            }

            PitayaClientLib.Logger.LogInfo("tcp__on_data_received - received data, req_id: " + message.Id);

            if (message.Id != 0)
            {
                /* request */
                PitayaInternalError error = null;
                if (message.Error)
                {
                    error = new PitayaServerError(message.Data);
                }
                _notifier.FireResponse(Client, message.Id, message.Data, ClientConfig.IsPollingEnabled, error, Serializer.Value);

                _sender.RemoveRequestFromResponsePendingQueue(message.Id);
            }
            else
            {
                _notifier.FirePush(Client, message.Route, message.Data, ClientConfig.IsPollingEnabled);
            }
        }

        private void OnKickReceived()
        {
            PitayaClientLib.Logger.LogInfo("tcp__on_kick_received - kicked by server");

            var kickedByServerEvent = new KickedByServerEvent();
            _notifier.FireEvent(Client, kickedByServerEvent, ClientConfig.IsPollingEnabled);

            Client.Disconnect();
        }


        public void AddEventHandler(Action<IClientEvent> onClientEvent)
        {
            _notifier.AddEventHandler(Client, onClientEvent);
        }

        public void SetPushHandler(Action<string, byte[]> onPush)
        {
            _notifier.SetPushHandler(Client, onPush);
        }

        public void Disconnect()
        {
            _reconnectionRetries = 0;
            var disconnectEvent = new DisconnectEvent();
            _notifier.FireEvent(Client, disconnectEvent, ClientConfig.IsPollingEnabled);
            Close();
        }

        public void Close()
        {
            if (_connectionOptions.Socket != null)
            {
                _connectionOptions.Socket.Close();
                _connectionOptions.Socket.Dispose();
                _connectionOptions.Socket = null;
            }

            _reader?.Close();

            if (_sender != null)
            {
                _sender.Dispose();
                _sender = null;
            }

            _heartbeatService?.Stop();
            _heartbeatService = null;

            State = TransportState.NotConnected;

            _handshakeService.InvokeCallback(null);
        }

    }
}
