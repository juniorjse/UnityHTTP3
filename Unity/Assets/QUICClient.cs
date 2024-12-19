using System.Net.Http;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Quic;
using System.Threading;
using System.Buffers;
using System.Collections;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt);
public unsafe delegate int StreamCallbackDelegate(QUIC_HANDLE* streamHandle, void* context, QUIC_STREAM_EVENT* streamEvent);
public class QUICClient : MonoBehaviour
{
    public Text _statusconnection;
    public Text _request;
    private GCHandle _callbackHandle;
    private unsafe QUIC_HANDLE* connection = null;
    private unsafe QUIC_HANDLE* registration = null;
    private unsafe QUIC_HANDLE* configuration = null;
    private unsafe QUIC_HANDLE* stream = null;
    private unsafe QUIC_API_TABLE* ApiTable;


    public void AndroidConnection()
    {
        if (Application.platform == RuntimePlatform.Android) 
        {
            AndroidJavaObject pluginObject = new AndroidJavaObject("com.example.quicconnectionwrapper.QuicInstructions");
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

            if (pluginObject != null)
            {
                _statusconnection.text = "Preste a fazer conexao";

                _statusconnection.text = pluginObject.Call<string>("QuicAndroidConnect", unityActivity);
                Debug.Log("Instância do plugin criada com sucesso.");
            }
            else
            {
                _statusconnection.text = ("Falha ao carregar o objeto");
            }
        }
        
    }

    public void ConnectToQUICUnity()
    {
        try
        {
            unsafe
            {
                ApiTable = MsQuic.Open();

                // Fixando o ponteiro para registro
                fixed (QUIC_HANDLE** pRegistration = &registration)
                {
                    MsQuic.ThrowIfFailure(ApiTable->RegistrationOpen(null, pRegistration));
                }

                // Configurando ALPN
                byte* alpn = stackalloc byte[] { (byte)'h', (byte)'3' }; // Usando HTTP/3
                QUIC_BUFFER buffer = new()
                {
                    Buffer = alpn,
                    Length = 2
                };

                QUIC_SETTINGS settings = new()
                {
                    IsSetFlags = 0,
                    IdleTimeoutMs = 10000
                };

                settings.IsSetFlags |= 0x00000001;

                settings.IsSet.IdleTimeoutMs = 1;

                settings.IsSet.PeerBidiStreamCount = 1;
                settings.IsSetFlags |= 0x00000002;

                settings.PeerBidiStreamCount = 1;

                settings.IsSetFlags |= 0x00000004;
                settings.IsSet.PeerUnidiStreamCount = 1;
                settings.PeerUnidiStreamCount = 3;

                // Fixando o ponteiro para configuração
                fixed (QUIC_HANDLE** pConfiguration = &configuration)
                {
                    MsQuic.ThrowIfFailure(ApiTable->ConfigurationOpen(registration, &buffer, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, pConfiguration));
                }

                QUIC_CREDENTIAL_CONFIG credConfig = new()
                {
                    Type = QUIC_CREDENTIAL_TYPE.NONE,
                    Flags = QUIC_CREDENTIAL_FLAGS.CLIENT | QUIC_CREDENTIAL_FLAGS.NO_CERTIFICATE_VALIDATION
                };
                MsQuic.ThrowIfFailure(ApiTable->ConfigurationLoadCredential(configuration, &credConfig));

                // Callback
                NativeCallbackDelegate clientConnectionCallback = ClientConnectionCallback;
                _callbackHandle = GCHandle.Alloc(clientConnectionCallback);
                IntPtr clientConnectionCallbackPtr = Marshal.GetFunctionPointerForDelegate(clientConnectionCallback);

                // Fixando o ponteiro para conexão
                fixed (QUIC_HANDLE** pConnection = &connection)
                {
                    MsQuic.ThrowIfFailure(ApiTable->ConnectionOpen(registration, (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>)clientConnectionCallbackPtr.ToPointer(), null, pConnection));
                }

                // Resolve DNS para "google.com" antes de se conectar
                var ipAddresses = Dns.GetHostAddresses("google.com");
                if (ipAddresses.Length == 0)
                {
                    throw new Exception("Falha ao resolver DNS.");
                }

                // Inicia a conexão
                sbyte* target = stackalloc sbyte[50];
                int written = Encoding.UTF8.GetBytes(ipAddresses[0].ToString(), new Span<byte>(target, 50));
                target[written] = 0;

                MsQuic.ThrowIfFailure(ApiTable->ConnectionStart(connection, configuration, (ushort)MsQuic.QUIC_ADDRESS_FAMILY_UNSPEC, target, 4567));
                Thread.Sleep(1000);
                _statusconnection.text = "Status: connected";
                Debug.Log("Connection started to google.com");
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
                if (connection == null)
                {
                    Debug.Log("No active connection to disconnect.");
                    return;
                }
                Thread.Sleep(1000);
                Debug.Log("Disconnecting...");
                ApiTable->ConnectionShutdown(connection, QUIC_CONNECTION_SHUTDOWN_FLAGS.NONE, 0);
                ApiTable->ConnectionClose(connection);
                connection = null;

                if (configuration != null)
                {
                    ApiTable->ConfigurationClose(configuration);
                    configuration = null;
                }
                if (registration != null)
                {
                    ApiTable->RegistrationClose(registration);
                    registration = null;
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

    public unsafe int StreamEventCallback(QUIC_HANDLE* streamHandle, void* context, QUIC_STREAM_EVENT* streamEvent)
    {
        switch (streamEvent->Type)
        {
            case QUIC_STREAM_EVENT_TYPE.RECEIVE:
                // Acesse o ponteiro para o array de buffers na estrutura RECEIVE
                QUIC_BUFFER* buffers = streamEvent->RECEIVE.Buffers;
                uint length = buffers->Length;  // Aqui obtemos o comprimento do buffer

                // Acesse os dados do buffer
                byte* receivedData = buffers->Buffer;

                // Converte os dados recebidos para um array de byte
                byte[] responseData = new byte[length];
                Marshal.Copy((IntPtr)receivedData, responseData, 0, (int)length);

                // Converte os dados para string
                string responseText = Encoding.ASCII.GetString(responseData);

            Debug.Log("Data received: " + responseText);
                Debug.Log("Data received on stream.");
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
                Debug.Log("All done! - Request");
                if (streamEvent->SHUTDOWN_COMPLETE.AppCloseInProgress == 0)
                {
                    ApiTable->StreamClose(stream);
                }
                break;
            case QUIC_STREAM_EVENT_TYPE.CANCEL_ON_LOSS:
                Debug.Log("Cancel On Lost");
                break;
            default:
                Debug.Log($"StreamEventCallback - Type: {streamEvent->Type}");
                Debug.Log("Other stream event.");
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
                    Debug.Log("Shut down by transport, " + evnt->SHUTDOWN_INITIATED_BY_TRANSPORT.Status);
                }
                break;
            case QUIC_CONNECTION_EVENT_TYPE.SHUTDOWN_INITIATED_BY_PEER:
                Debug.Log("Shut down by peer, " + (ulong)evnt->SHUTDOWN_INITIATED_BY_PEER.ErrorCode);
                break;
            case QUIC_CONNECTION_EVENT_TYPE.SHUTDOWN_COMPLETE:
                Debug.Log("All done! - Conexao");
                if (evnt->SHUTDOWN_COMPLETE.AppCloseInProgress == 0)
                {
                    ApiTable->ConnectionClose(connection);
                }
                break;

        }
        if (evnt->Type == QUIC_CONNECTION_EVENT_TYPE.CONNECTED)
        {
            Debug.Log("Connection established.");
        }

        if (evnt->Type == QUIC_CONNECTION_EVENT_TYPE.PEER_STREAM_STARTED)
        {
            return MsQuic.QUIC_STATUS_ABORTED;
        }

        return MsQuic.QUIC_STATUS_SUCCESS;
    }

    // Send the HTTP/3 request over QUIC
    public unsafe void Request()
    {
        try
        {
            string url = "www.google.com";
            byte[] requestBytes = Encoding.ASCII.GetBytes(url);

            // Callback
            StreamCallbackDelegate streamEventCallback = StreamEventCallback;
            _callbackHandle = GCHandle.Alloc(streamEventCallback);
            IntPtr streamEventCallbackPtr = Marshal.GetFunctionPointerForDelegate(streamEventCallback);


            fixed (QUIC_HANDLE** pStream = &stream)
            {
                MsQuic.ThrowIfFailure(ApiTable->StreamOpen(connection, QUIC_STREAM_OPEN_FLAGS.ZERO_RTT, (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, int>)streamEventCallbackPtr.ToPointer(), null, pStream));
            }

            Debug.Log("Starting...");

            MsQuic.ThrowIfFailure(ApiTable->StreamStart(stream, QUIC_STREAM_START_FLAGS.NONE));
            
            // Send the HTTP GET request through the QUIC stream
            QUIC_BUFFER buffer = new()
            {
                Buffer = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(requestBytes, 0),
                Length = (uint)requestBytes.Length
            };

            Debug.Log("Sending data...");

            // Send the data through the QUIC stream
            MsQuic.ThrowIfFailure(ApiTable->StreamSend(stream, &buffer, 1, QUIC_SEND_FLAGS.NONE, &buffer));

            Debug.Log("Request sent");
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
   
}
