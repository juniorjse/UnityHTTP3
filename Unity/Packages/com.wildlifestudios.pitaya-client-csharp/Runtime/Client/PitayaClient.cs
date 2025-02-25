using System;
using System.IO;
using System.Collections.Generic;
using Google.Protobuf;
using Wildlife.PitayaCSharp.Serializer;
using Wildlife.PitayaCSharp.Logger;
using Wildlife.PitayaCSharp.Transport;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Event;
using Wildlife.PitayaCSharp.Error;
using Wildlife.PitayaCSharp.QueueDispatcher;

namespace Wildlife.PitayaCSharp.Client
{
    public class PitayaClient : IPitayaClient, IPitayaListener, IDisposable
    {
        public const byte DynamicAllocationFlag = 0x0;
        public const int DefaultConnectionTimeout = 30;
        public const int WithoutTimeout = -1;
        public const int AlwaysRetry = -1;

        public event Action<PitayaNetWorkState, NetworkError> NetWorkStateChangedEvent;
        public ISerializerFactory SerializerFactory { get; set; }
        EventManager _eventManager;
        bool _disposed;
        ITransporter _transporter;
        readonly object _stateLock;
        PitayaClientState _state;
        // TODO: Should this be static?
        public static IPitayaQueueDispatcher QueueDispatcher { get; set; }
        public IPitayaClientConfig Config { get; private set; }
        bool _isLibInitialized;
        bool _isInPoll;
        uint _sequenceNumber;
        uint _nextRequestUid;

        public PitayaClient() : this(false) { }
        public PitayaClient(TransporterName transporterName) : this(false, transporterName: transporterName) { }
        public PitayaClient(int connectionTimeout) : this(false, null, connectionTimeout: connectionTimeout) { }
        public PitayaClient(string certificateName = null) : this(false, certificateName: certificateName) { }
        public PitayaClient(bool enableReconnect = false, string certificateName = null, int connectionTimeout = DefaultConnectionTimeout, IPitayaQueueDispatcher queueDispatcher = null, TransporterName transporterName = TransporterName.TCP)
        {
            queueDispatcher ??= MainQueueDispatcher.GetInstance();
            QueueDispatcher = queueDispatcher;
            _stateLock = new object();

            if (!_isLibInitialized) InitializeLib();
            SetLogLevel(PitayaLogLevel.Debug);
            Init(certificateName, certificateName != null, false, enableReconnect, connectionTimeout, new SerializerFactory(), transporterName);
        }

        ~PitayaClient()
        {
            Dispose();
        }

        private void Init(string certificateName, bool enableTlS, bool enablePolling, bool enableReconnect, int connTimeoutInSeconds, ISerializerFactory serializerFactory, TransporterName transporterName)
        {
            SerializerFactory = serializerFactory;
            _eventManager = new EventManager();

            var config = new DefaultPitayaClientConfig(
                    isPollingEnabled: enablePolling,
                    isAutomaticReconnectionEnabled: enableReconnect,
                    connectionTimeoutInSeconds: connTimeoutInSeconds,
                    clientTransporterName: transporterName);

            Init(config);

            _transporter.AddEventHandler(OnEvent);
            _transporter.SetPushHandler(OnPush);

            if (certificateName != null)
                SetCertificatePath(certificateName);
        }

        private void Init(IPitayaClientConfig clientConfig)
        {
            if (clientConfig is null)
                Config = new DefaultPitayaClientConfig();
            else
                Config = clientConfig;

            TransporterPluginRepository transporterPluginRepository = TransporterPluginRepository.Instance;
            ITransporterPlugin transporterPlugin = transporterPluginRepository.Get(Config.ClientTransporterName);

            if (transporterPlugin == null)
            {
                PitayaClientLib.Logger.LogError(string.Format("pc_client_init - no registered transport plugin found, "
                + "transport plugin: {0}", Config.ClientTransporterName));
                throw new PitayaNoTransporterException();
            }

            _transporter = transporterPlugin.CreateTransporter(this, Config);

            if (_transporter == null)
            {
                PitayaClientLib.Logger.LogError("pc_client_init - create/init transport error");
                throw new PitayaCommonErrorException("Fail to create a client");
            }

            _sequenceNumber = 0;
            _nextRequestUid = 1;

            _isInPoll = false;
            _state = PitayaClientState.Inited;

            PitayaClientLib.Logger.LogDebug("pc_client_init - init ok");
        }

        private void InitializeLib()
        {
            PitayaClientLib.Init();
            _isLibInitialized = true;
        }

        private static string FindCertPathFromName(string name)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string searchDirectory = Path.Combine(baseDirectory, "Assets", "StreamingAssets");

            try
            {
                string[] files = Directory.GetFiles(searchDirectory, name, SearchOption.AllDirectories);

                if (files.Length > 0)
                    return files[0];
            }
            catch { }

            return null;
        }

        public static void SetCertificateName(string name)
        {
            var certPath = FindCertPathFromName(name);
            TLSTransporterPlugin.SetCAFile(certPath);
        }

        public static void SetCertificatePath(string path)
        {
            TLSTransporterPlugin.SetCAFile(path);
        }

        public static void SetLogLevel(PitayaLogLevel level)
        {
            PitayaClientLib.SetLogLevel(level);
        }

        public static void SetLogFunction(LogFunction logFunction)
        {
            PitayaClientLib.SetLogFunction(logFunction);
        }

        public int Quality
        {
            get { return _transporter.Quality; }
        }

        public PitayaClientState State // thread-safe
        {
            get { lock (_stateLock) { return _state; } }

            internal set { lock (_stateLock) { _state = value; } }
        }

        public void Connect(string host, int port, Dictionary<string, string> handshakeOpts)
        {
            string opts = SimpleJson.SimpleJson.SerializeObject(handshakeOpts);
            Connect(host, port, opts);
        }

        public void Connect(string host, int port, string handshakeOpts = null)
        {
            if (host == null || port < 0 || port > (1 << 16) - 1)
            {
                PitayaClientLib.Logger.LogError("pc_client_connect - invalid args");
                throw new PitayaInvalidArgumentException("Error when Connect was called");
            }

            // TODO: should do polling, if enabled in config?

            switch (State)
            {
                case PitayaClientState.Disconnecting:
                    PitayaClientLib.Logger.LogError("pc_client_connect - invalid state, state: " + State);
                    throw new PitayaInvalidStateException("Error when Connect was called");

                case PitayaClientState.Connecting:
                case PitayaClientState.Connected:
                    PitayaClientLib.Logger.LogInfo("pc_client_connect - client already connecting or connected");
                    return;

                case PitayaClientState.Inited:
                    State = PitayaClientState.Connecting;
                    try
                    {
                        _transporter.Connect(host, port, handshakeOpts);
                    }
                    catch (PitayaException pitayaException)
                    {
                        PitayaClientLib.Logger.LogError("pc_client_connect - transport connect error, rc: " + pitayaException);
                        State = PitayaClientState.Inited;

                        if (pitayaException is PitayaInvalidJsonException)
                            throw new Exception("Cannot connect: invalid handshake options json data", pitayaException);
                        throw new Exception("Error when Connect was called", pitayaException);
                    }
                    return;
                default:
                    PitayaClientLib.Logger.LogError("pc_client_connect - unknown client state found, state: " + State);
                    throw new Exception("Error when Connect was called", new PitayaCommonErrorException());
            }
        }

        /// <summary cref="Request&lt;TResponse&gt;(string, object, Action&lt;TResponse&gt;, Action&lt;PitayaError&gt;, int)">
        /// </summary>
        public void Request<TResponse>(string route, object msg, Action<TResponse> action, Action<PitayaError> errorAction, int timeout = WithoutTimeout)
        {
            IPitayaSerializer serializer = SerializerFactory.CreateJsonSerializer();
            if (msg is IMessage) serializer = SerializerFactory.CreateProtobufSerializer(_transporter.Serializer.Value);
            RequestInternal(route, msg, timeout, serializer, action, errorAction);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Request&lt;TResponse&gt;(string, object, Action&lt;TResponse&gt;, Action&lt;PitayaError&gt;, int)"/> instead.</para>
        /// </summary>
        public void Request(string route, Action<string> action, Action<PitayaError> errorAction)
        {
            Request(route, (string)null, action, errorAction);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Request&lt;TResponse&gt;(string, object, Action&lt;TResponse&gt;, Action&lt;PitayaError&gt;, int)"/> instead.</para>
        /// </summary>
        public void Request<T>(string route, Action<T> action, Action<PitayaError> errorAction)
        {
            Request(route, null, action, errorAction);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Request&lt;TResponse&gt;(string, object, Action&lt;TResponse&gt;, Action&lt;PitayaError&gt;, int)"/> instead.</para>
        /// </summary>
        public void Request(string route, string msg, Action<string> action, Action<PitayaError> errorAction)
        {
            Request(route, msg, WithoutTimeout, action, errorAction);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Request&lt;TResponse&gt;(string, object, Action&lt;TResponse&gt;, Action&lt;PitayaError&gt;, int)"/> instead.</para>
        /// </summary>
        public void Request<T>(string route, IMessage msg, Action<T> action, Action<PitayaError> errorAction)
        {
            Request(route, msg, WithoutTimeout, action, errorAction);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Request&lt;TResponse&gt;(string, object, Action&lt;TResponse&gt;, Action&lt;PitayaError&gt;, int)"/> instead.</para>
        /// </summary>
        public void Request<T>(string route, IMessage msg, int timeout, Action<T> action, Action<PitayaError> errorAction)
        {
            ProtobufSerializer.SerializationFormat format = _transporter.Serializer.Value;
            RequestInternal(route, msg, timeout, SerializerFactory.CreateProtobufSerializer(format), action, errorAction);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Request&lt;TResponse&gt;(string, object, Action&lt;TResponse&gt;, Action&lt;PitayaError&gt;, int)"/> instead.</para>
        /// </summary>
        public void Request(string route, string msg, int timeout, Action<string> action, Action<PitayaError> errorAction)
        {
            RequestInternal(route, msg, timeout, new LegacyJsonSerializer(), action, errorAction);
        }

        void RequestInternal<TResponse, TRequest>(string route, TRequest msg, int timeout, IPitayaSerializer serializer, Action<TResponse> onSuccess, Action<PitayaError> onError)
        {
            uint currentRequestUid = _nextRequestUid++;

            Action<byte[]> onResponse = res => { onSuccess(serializer.Decode<TResponse>(res)); };

            _eventManager.AddCallBack(currentRequestUid, onResponse, onError);

            try
            {
                if (route == null)
                {
                    PitayaClientLib.Logger.LogError("pc_request_with_timeout - invalid args");
                    throw new PitayaInvalidArgumentException();
                }

                if (State != PitayaClientState.Connected && State != PitayaClientState.Connecting)
                {
                    PitayaClientLib.Logger.LogError("pc_request_with_timeout - invalid state, state: " + State);
                    throw new PitayaInvalidStateException();
                }

                if (timeout != WithoutTimeout && timeout <= 0)
                {
                    PitayaClientLib.Logger.LogError("pc_request_with_timeout - timeout value is invalid");
                    throw new PitayaInvalidArgumentException();
                }

                uint requestSequenceNumber = _sequenceNumber++;

                try
                {
                    PitayaClientLib.Logger.LogInfo("pc_request_with_timeout - add request to queue, req id: " + currentRequestUid);
                    _transporter.Send(MessageType.Request, route, _sequenceNumber, serializer.Encode(msg), currentRequestUid, timeout);
                    PitayaClientLib.Logger.LogDebug("pc_request_with_timeout - transport send function CALLED");
                }
                catch (PitayaException pitayaException)
                {
                    PitayaClientLib.Logger.LogDebug("pc_request_with_timeout - transport send function CALLED");
                    PitayaClientLib.Logger.LogError(string.Format("pc_request_with_timeout - send to transport error,"
                    + " req id: {0}, error: {1}", currentRequestUid, pitayaException));
                    throw;
                }
            }
            catch (PitayaException pitayaException)
            {
                PitayaClientLib.Logger.LogDebug(string.Format("request - failed to perform request {0}", pitayaException));
                OnRequestError(currentRequestUid, new PitayaError(pitayaException.ToString(), "Failed to send request"));
            }
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Notify(string, object, int)"/> instead.</para>
        /// </summary>
        public void Notify(string route, IMessage msg)
        {
            Notify(route, WithoutTimeout, msg);
        }

        /// <summary cref="Notify(string, object, int)">
        /// </summary>
        public void Notify(string route, object msg, int timeout = WithoutTimeout)
        {
            IPitayaSerializer serializer = SerializerFactory.CreateJsonSerializer();
            if (msg is IMessage) serializer = SerializerFactory.CreateProtobufSerializer(_transporter.Serializer.Value);
            NotifyInternal(route, msg, serializer, timeout);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Notify(string, object, int)"/> instead.</para>
        /// </summary>
        public void Notify(string route, int timeout, IMessage msg)
        {
            ProtobufSerializer.SerializationFormat format = _transporter.Serializer.Value;
            NotifyInternal(route, msg, SerializerFactory.CreateProtobufSerializer(format), timeout);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Notify(string, object, int)"/> instead.</para>
        /// </summary>
        public void Notify(string route, string msg)
        {
            Notify(route, WithoutTimeout, msg);
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="Notify(string, object, int)"/> instead.</para>
        /// </summary>
        public void Notify(string route, int timeout, string msg)
        {
            NotifyInternal(route, msg, new LegacyJsonSerializer(), timeout);
        }

        void NotifyInternal(string route, object msg, IPitayaSerializer serializer, int timeout = WithoutTimeout)
        {
            if (route == null || msg == null)
            {
                PitayaClientLib.Logger.LogError("pc_notify_with_timeout - invalid args");
                return;
            }

            if (timeout != WithoutTimeout && timeout <= 0)
            {
                PitayaClientLib.Logger.LogError("pc_notify_with_timeout - invalid timeout value");
                return;
            }

            if (State != PitayaClientState.Connected && State != PitayaClientState.Connecting)
            {
                PitayaClientLib.Logger.LogError("pc_notify_with_timeout - invalid state, state: " + State);
                return;
            }

            uint notifySequenceNumber = _sequenceNumber++;
            PitayaClientLib.Logger.LogInfo("pc_notify_with_timeout - add notify to queue, seq num: " + notifySequenceNumber);

            try
            {
                _transporter.Send(MessageType.Notify, route, notifySequenceNumber, serializer.Encode(msg), TransporterConstants.NotifyPushRequestUid, timeout);
            }
            catch (Exception exception)
            {
                PitayaClientLib.Logger.LogError(string.Format("pc_notify_with_timeout - send to transport error,"
                + " seq num: {0}, error: {1}", notifySequenceNumber, exception.Message));
            }
        }

        /// <summary>
        /// <para>DEPRECATED. Use <see cref="OnRoute&lt;T&gt;(string, Action&lt;T&gt;)"/> instead.</para>
        /// </summary>
        public void OnRoute(string route, Action<string> action)
        {
            OnRouteInternal(route, action, new LegacyJsonSerializer());
        }

        /// <summary cref="OnRoute&lt;T&gt;(string, Action&lt;T&gt;)">
        /// </summary>
        public void OnRoute<T>(string route, Action<T> action)
        {
            IPitayaSerializer serializer = SerializerFactory.CreateJsonSerializer();
            if (typeof(IMessage).IsAssignableFrom(typeof(T))) serializer = SerializerFactory.CreateProtobufSerializer(_transporter.Serializer.Value);

            OnRouteInternal(route, action, serializer);
        }

        private void OnRouteInternal<T>(string route, Action<T> action, IPitayaSerializer serializer)
        {
            Action<byte[]> responseAction = res => { action(serializer.Decode<T>(res)); };
            _eventManager.AddOnRouteEvent(route, responseAction);
        }

        public void OffRoute(string route)
        {
            _eventManager.RemoveOnRouteEvent(route);
        }

        private void OnEvent(IClientEvent clientEvent)
        {
            State = clientEvent.GetNextClientState();
            PitayaClientLib.Logger.LogDebug(string.Format("OnEvent - pinvoke callback START | ev={0} client={1}", clientEvent, this));

            string error = clientEvent.Arg1;
            string description = clientEvent.Arg2;

            if (error != null)
            {
                PitayaClientLib.Logger.LogDebug("OnEvent - msg=" + error);
            }

            QueueDispatcher.Dispatch(() =>
            {
                if (clientEvent is INetworkEvent networkEvent)
                {
                    NetworkError networkError = null;

                    if (error != null) networkError = new NetworkError(error, description);

                    OnNetworkEvent(networkEvent.GetNextNetworkState(), networkError);
                }

                PitayaClientLib.Logger.LogDebug("OnEvent - main thread END");
            });

            PitayaClientLib.Logger.LogDebug("OnEvent - pinvoke callback END");
        }

        private void OnPush(string route, byte[] messageData)
        {
            if (messageData == null)
            {
                PitayaClientLib.Logger.LogError("pc__trans_push - error parameters");
                return;
            }

            if (messageData.Length == 0)
            {

                PitayaClientLib.Logger.LogError("pc__trans_push - empty buffer");
                return;
            }

            PitayaClientLib.Logger.LogInfo("pc__trans_push - route: " + route);
            QueueDispatcher.Dispatch(() =>
            {
                OnUserDefinedPush(route, messageData);
            });
        }


        // Disconnect disconnects the client
        public void Disconnect()
        {
            var state = State;

            switch (state)
            {
                case PitayaClientState.Inited:
                    PitayaClientLib.Logger.LogError("pc_client_disconnect - invalid state, state: " + state);
                    break;

                case PitayaClientState.Connecting:
                case PitayaClientState.Connected:
                    State = PitayaClientState.Disconnecting;
                    try
                    {
                        _transporter.Disconnect();
                    }
                    catch (PitayaException pitayaException)
                    {
                        PitayaClientLib.Logger.LogError(string.Format("pc_client_disconnect - transport disconnect error: {0}",
                        pitayaException));
                        State = state;
                    }
                    break;

                case PitayaClientState.Disconnecting:
                    PitayaClientLib.Logger.LogInfo("pc_client_disconnect - client is already disconnecting");
                    break;

                default:
                    PitayaClientLib.Logger.LogError("pc_client_disconnect - unknown client state found, " + state);
                    break;
            }
        }

        //---------------Pitaya Listener------------------------//

        public void OnRequestResponse(uint rid, byte[] data)
        {
            _eventManager.InvokeCallBack(rid, data);
        }

        public void OnRequestError(uint rid, PitayaError error)
        {
            _eventManager.InvokeErrorCallBack(rid, error);
        }

        public void OnNetworkEvent(PitayaNetWorkState state, NetworkError error = null)
        {
            if (NetWorkStateChangedEvent != null) NetWorkStateChangedEvent.Invoke(state, error);
        }

        public void OnUserDefinedPush(string route, byte[] serializedBody)
        {
            _eventManager.InvokeOnEvent(route, serializedBody);
        }

        public void Dispose()
        {
            PitayaClientLib.Logger.LogDebug("PitayaClient Disposed " + this);
            if (_disposed)
                return;

            if (_eventManager != null) _eventManager.Dispose();
            _eventManager = null;

            _nextRequestUid = 1;
            _transporter.Disconnect();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public void ClearAllCallbacks()
        {
            _eventManager.ClearAllCallbacks();
        }

        public void RemoveAllOnRouteEvents()
        {
            _eventManager.RemoveAllOnRouteEvents();
        }
    }
}
