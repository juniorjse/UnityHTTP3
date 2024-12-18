using System.Net.Http;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Quic;
using System.Threading;
using UnityEditor.MemoryProfiler;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt);
public unsafe delegate int StreamCallbackDelegate(QUIC_HANDLE* streamHandle, void* context, QUIC_STREAM_EVENT* streamEvent);
public class QUICClient : MonoBehaviour
{
    public Text _statusconnection;
    public Text _request;
    private GCHandle _callbackHandle;
    private unsafe QUIC_HANDLE* _connection = null;
    private unsafe QUIC_HANDLE* _registration = null;
    private unsafe QUIC_HANDLE* _configuration = null;
    private unsafe QUIC_HANDLE* _stream = null;
    private unsafe QUIC_API_TABLE* ApiTable;
    ushort _port = 443;

    public void ConnectVerify()
    {
#if UNITY_IOS
        connectToQUIC();
#else
        ConnectToQUICUnity();
#endif
    }

    public void DisconnectVerify()
    {
#if UNITY_IOS
        disconnectFromQUIC();
#else
        DisconnectFromUnityQUIC();
#endif
    }

    public void RequestVerify()
    {
#if UNITY_IOS
        getRequestToServer();
#else
        Request();
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void connectToQUIC();

    [DllImport("__Internal")]
    private static extern void disconnectFromQUIC();

    [DllImport("__Internal")]
    private static extern void getRequestToServer();
#else

    unsafe void LoadConfiguration(bool isUnsecureConnection)
    {
        QUIC_SETTINGS settings = new()
        {
            IsSetFlags = 0
        };

        //
        // Configures the client's idle timeout.
        //
        settings.IdleTimeoutMs = 30000;
        settings.IsSet.IdleTimeoutMs = 1;

        settings.PeerBidiStreamCount = 1;
        settings.IsSet.PeerBidiStreamCount = 1;

        settings.IsSet.PeerUnidiStreamCount = 1;
        settings.PeerUnidiStreamCount = 3;
        //
        // Configures a default client configuration, optionally disabling
        // server certificate validation.
        //
        QUIC_CREDENTIAL_CONFIG credConfig = new()
        {
            Type = QUIC_CREDENTIAL_TYPE.NONE,
            Flags = QUIC_CREDENTIAL_FLAGS.CLIENT
        };

        if (isUnsecureConnection)
        {
            credConfig.Flags |= QUIC_CREDENTIAL_FLAGS.NO_CERTIFICATE_VALIDATION;
        }

        // Configures ALPN
        byte* alpn = stackalloc byte[] { (byte)'h', (byte)'3' }; // Usando HTTP/3

        QUIC_BUFFER buffer = new()
        {
            Buffer = alpn,
            Length = 2
        };

        //
        // Allocate/initialize the configuration object, with the configured ALPN
        // and settings.
        //
        fixed (QUIC_HANDLE** pConfiguration = &_configuration)
        {
            MsQuic.ThrowIfFailure(ApiTable->ConfigurationOpen(_registration, &buffer, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, pConfiguration));
        }

        //
        // Loads the TLS credential part of the configuration. This is required even
        // on client side, to indicate if a certificate is required or not.
        //
        MsQuic.ThrowIfFailure(ApiTable->ConfigurationLoadCredential(_configuration, &credConfig));
    }

    public void ConnectToQUICUnity()
    {
        try
        {
            unsafe
            {
                //
                // Open a handle to the library and get the API function table.
                //
                ApiTable = MsQuic.Open();

                //
                // Create a registration for the app's connections.
                //
                fixed (QUIC_HANDLE** pRegistration = &_registration)
                {
                    MsQuic.ThrowIfFailure(ApiTable->RegistrationOpen(null, pRegistration));
                }

                //
                // Load the client configuration based on the 'unsecure' value
                //
                LoadConfiguration(true);

                // Callback
                NativeCallbackDelegate clientConnectionCallback = ClientConnectionCallback;
                _callbackHandle = GCHandle.Alloc(clientConnectionCallback);
                IntPtr clientConnectionCallbackPtr = Marshal.GetFunctionPointerForDelegate(clientConnectionCallback);

                //
                // Allocate a new connection object.
                //
                fixed (QUIC_HANDLE** pConnection = &_connection)
                {
                    MsQuic.ThrowIfFailure(ApiTable->ConnectionOpen(_registration, (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>)clientConnectionCallbackPtr.ToPointer(), null, pConnection));
                }

                string server = "127.0.0.1";
                int targetLength = Encoding.UTF8.GetByteCount(server);

                sbyte* target = stackalloc sbyte[targetLength + 1];
                int written = Encoding.UTF8.GetBytes(server, new Span<byte>(target, targetLength));
                target[written] = 0;

                MsQuic.ThrowIfFailure(ApiTable->ConnectionStart(_connection, _configuration, (ushort)MsQuic.QUIC_ADDRESS_FAMILY_UNSPEC, target, 8081));
                Thread.Sleep(1000);
                _statusconnection.text = "Status: connected";
                Debug.Log("Connection started to " + server);
            }
        }
        catch (Exception ex)
        {
            _statusconnection.text = $"Status: {ex.Message}";
            Debug.LogError($"Erro ao conectar: {ex.Message}");
        }
    }

    public void DisconnectFromUnityQUIC()
    {
        try
        {
            unsafe
            {
                if (_connection == null)
                {
                    Debug.Log("No active connection to disconnect.");
                    return;
                }
                Thread.Sleep(1000);
                Debug.Log("Disconnecting...");
                ApiTable->ConnectionShutdown(_connection, QUIC_CONNECTION_SHUTDOWN_FLAGS.NONE, 0);
                ApiTable->ConnectionClose(_connection);
                _connection = null;

                if (_configuration != null)
                {
                    ApiTable->ConfigurationClose(_configuration);
                    _configuration = null;
                }
                if (_registration != null)
                {
                    ApiTable->RegistrationClose(_registration);
                    _registration = null;
                }

                if (_callbackHandle.IsAllocated)
                {
                    _callbackHandle.Free();
                }

                _statusconnection.text = "Status: disconnected";
                Debug.Log("Disconnected successfully.");
            }
        }
        catch (Exception ex)
        {
            _statusconnection.text = $"Status: {ex.Message}";
            Debug.LogError($"Erro ao desconectar: {ex.Message}");
        }
    }

    public void OnResponse(string resp)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            _request.text = "Response: " + resp;
        }
        );
    }

    public unsafe int StreamEventCallback(QUIC_HANDLE* streamHandle, void* context, QUIC_STREAM_EVENT* streamEvent)
    {
        switch (streamEvent->Type)
        {
            case QUIC_STREAM_EVENT_TYPE.RECEIVE:
                Debug.Log("Data received on stream.");
                var receiveBuffer = streamEvent->RECEIVE.Buffers;

                if (receiveBuffer != null && receiveBuffer->Length > 0)
                {
                    byte[] receivedData = new byte[receiveBuffer->Length];
                    Marshal.Copy((IntPtr)receiveBuffer->Buffer, receivedData, 0, (int)receiveBuffer->Length);

                    Debug.Log($"Data received: {BitConverter.ToString(receivedData)}");

                    // Decode the received bytes, as it is a normal text, it's enough to decode with utf8
                    string receivedString = Encoding.UTF8.GetString(receivedData);

                    OnResponse(receivedString);
                }
                break;
            case QUIC_STREAM_EVENT_TYPE.SEND_COMPLETE:
                Debug.Log("Data Sent.");
                break;
            case QUIC_STREAM_EVENT_TYPE.PEER_SEND_ABORTED:
                Debug.Log("Peer aborted!");
                break;
            case QUIC_STREAM_EVENT_TYPE.PEER_SEND_SHUTDOWN:
                Debug.Log("Peer shutdown.");
                break;
            case QUIC_STREAM_EVENT_TYPE.SHUTDOWN_COMPLETE:
                Debug.Log("All done!");
                if (streamEvent->SHUTDOWN_COMPLETE.AppCloseInProgress == 0)
                {
                    ApiTable->StreamClose(_stream);
                }
                break;
            default:
                Debug.Log("Other stream event: " + streamEvent->Type);
                break;
        }

        return 0;
    }

    private unsafe int ClientConnectionCallback(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt)
    {
        switch (evnt->Type)
        {
            case QUIC_CONNECTION_EVENT_TYPE.CONNECTED:
                Debug.Log("Connection established!");
                break;
            case QUIC_CONNECTION_EVENT_TYPE.SHUTDOWN_INITIATED_BY_TRANSPORT:
                if (evnt->SHUTDOWN_INITIATED_BY_TRANSPORT.Status == MsQuic.QUIC_STATUS_CONNECTION_IDLE)
                {
                    Debug.Log("Successfully shut down on idle.");
                }
                else
                {
                    Debug.Log("Shut down by transport: " + evnt->SHUTDOWN_INITIATED_BY_TRANSPORT.Status);
                    return evnt->SHUTDOWN_INITIATED_BY_TRANSPORT.Status;
                }
                break;
            case QUIC_CONNECTION_EVENT_TYPE.SHUTDOWN_INITIATED_BY_PEER:
                Debug.Log("Shut down by peer, " + evnt->SHUTDOWN_INITIATED_BY_PEER.ErrorCode);
                break;
            case QUIC_CONNECTION_EVENT_TYPE.SHUTDOWN_COMPLETE:
                Debug.Log("All done!");
                if (evnt->SHUTDOWN_COMPLETE.AppCloseInProgress == 0)
                {
                    ApiTable->ConnectionClose(_connection);
                }
                break;
            case QUIC_CONNECTION_EVENT_TYPE.RESUMPTION_TICKET_RECEIVED:
                Debug.Log("Resumption ticket received " + evnt->RESUMPTION_TICKET_RECEIVED.ResumptionTicketLength);
                for (int i = 0; i < evnt->RESUMPTION_TICKET_RECEIVED.ResumptionTicketLength; i++)
                {
                    Debug.Log(evnt->RESUMPTION_TICKET_RECEIVED.ResumptionTicket[i]);
                }
                break;
            case QUIC_CONNECTION_EVENT_TYPE.PEER_STREAM_STARTED:
                //
                // The peer has started/created a new stream. The app MUST set the
                // callback handler before returning.
                //
                Debug.Log("Peer started.");

                StreamCallbackDelegate streamEventCallback = StreamEventCallback;
                _callbackHandle = GCHandle.Alloc(streamEventCallback);
                IntPtr streamEventCallbackPtr = Marshal.GetFunctionPointerForDelegate(streamEventCallback);
                ApiTable->SetCallbackHandler(evnt->PEER_STREAM_STARTED.Stream, streamEventCallbackPtr.ToPointer(), null);
                break;
            default:
                Debug.Log("Other connection event: " + evnt->Type);
                break;

        }

        return MsQuic.QUIC_STATUS_SUCCESS;
    }

    // Send the HTTP/3 request over QUIC
    private unsafe void Request()
    {
        try
        {
            string url = "GET /search?q=WildlifeStudios HTTP/3.0\r\nHost: 127.0.0.1\r\n\r\n";  // A more valid HTTP request
            byte[] requestBytes = Encoding.ASCII.GetBytes(url);

            // Callback
            StreamCallbackDelegate streamEventCallback = StreamEventCallback;
            _callbackHandle = GCHandle.Alloc(streamEventCallback);
            IntPtr streamEventCallbackPtr = Marshal.GetFunctionPointerForDelegate(streamEventCallback);


            fixed (QUIC_HANDLE** pStream = &_stream)
            {
                MsQuic.ThrowIfFailure(ApiTable->StreamOpen(_connection, QUIC_STREAM_OPEN_FLAGS.NONE, (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, int>)streamEventCallbackPtr.ToPointer(), null, pStream), "StreamOpen failed.");
            }

            Debug.Log("Starting...");

            MsQuic.ThrowIfFailure(ApiTable->StreamStart(_stream, QUIC_STREAM_START_FLAGS.NONE), "StreamStart failed.");


            // Send the HTTP GET request through the QUIC stream
            fixed (byte* requestPtr = requestBytes)
            {
                QUIC_BUFFER buffer = new()
                {
                    Buffer = requestPtr,
                    Length = (uint)requestBytes.Length
                };

                Debug.Log("Sending data...");

                // Send the HTTP GET request through the QUIC stream
                MsQuic.ThrowIfFailure(ApiTable->StreamSend(_stream, &buffer, 1, QUIC_SEND_FLAGS.NONE, null), "StreamSend failed.");

                Debug.Log("Request sent.");
            }
        }
        catch (Exception ex)
        {
            _statusconnection.text = $"Error sending request: {ex.Message}";
            Debug.LogError($"Error sending request: {ex.Message}");
        }
    }

    // Prepare HTTP/3 GET request headers
    private byte[] PrepareHttp3RequestHeaders(string host, string path)
    {
        // HTTP/3 headers for a simple GET request
        string requestHeaders = $"GET {path} HTTP/3\r\n" +
                                $":method: GET\r\n" +
                                $":scheme: https\r\n" +
                                $":authority: {host}\r\n" +
                                "accept: text/html\r\n\r\n";

        return Encoding.UTF8.GetBytes(requestHeaders);
    }
#endif
}