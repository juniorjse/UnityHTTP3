using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Quic;
using System.Text;
using System.Threading;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt);

public class QUICClient : MonoBehaviour
{
    public Text _statusconnection;
    public Text _request;
    private GCHandle _callbackHandle;

    public void ConnectVerify()
    {
#if UNITY_IOS
        ConnectToQUIC("www.google.com", 443);
#else
        ConnectToQUICUnity();
#endif
    }

    public void DisconnectVerify()
    {
#if UNITY_IOS
        DisconnectFromQUIC();
#else
        DisconnectFromUnityQUIC();
#endif
    }

    public void RequestVerify()
    {
#if UNITY_IOS
        GetRequestToServer($"https://www.google.com/search?q=WildlifeStudios&tbm=nws");
#else
        Request();
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal", EntryPoint = "connectToQUIC")]
    private static extern void connectToQUIC(string host, int port);

    [DllImport("__Internal", EntryPoint = "disconnectFromQUIC")]
    private static extern void disconnectFromQUIC();

    [DllImport("__Internal", EntryPoint = "getRequestToServer")]
    private static extern void getRequestToServer(string url);

    public void ConnectToQUIC(string host, int port)
    {
        try
        {
            connectToQUIC(host, port);
            Debug.Log("Status: connected");
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
        }
    }

    public void DisconnectFromQUIC()
    {
        try
        {
            disconnectFromQUIC();
            Debug.Log("Status: disconnected");
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
        }
    }

    public void GetRequestToServer(string url)
    {
        try
        {
            getRequestToServer(url);
            Debug.Log("Request sent. Waiting for response...");
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
        }
    }
#else
    public void ConnectToQUICUnity()
    {
        try
        {
            unsafe
            {
                var ApiTable = MsQuic.Open();
                QUIC_HANDLE* registration = null;
                QUIC_HANDLE* configuration = null;
                QUIC_HANDLE* connection = null;

                try
                {
                    MsQuic.ThrowIfFailure(ApiTable->RegistrationOpen(null, &registration));

                    byte* alpn = stackalloc byte[] { (byte)'h', (byte)'3' };
                    QUIC_BUFFER buffer = new()
                    {
                        Buffer = alpn,
                        Length = 2
                    };

                    QUIC_SETTINGS settings = new()
                    {
                        IsSetFlags = 0
                    };
                    settings.IsSet.PeerBidiStreamCount = 1;
                    settings.PeerBidiStreamCount = 1;
                    settings.IsSet.PeerUnidiStreamCount = 1;
                    settings.PeerUnidiStreamCount = 3;

                    MsQuic.ThrowIfFailure(ApiTable->ConfigurationOpen(registration, &buffer, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, &configuration));

                    QUIC_CREDENTIAL_CONFIG config = new()
                    {
                        Flags = QUIC_CREDENTIAL_FLAGS.CLIENT
                    };
                    MsQuic.ThrowIfFailure(ApiTable->ConfigurationLoadCredential(configuration, &config));

                    // Crie uma instância do delegate
                    NativeCallbackDelegate callbackDelegate = NativeCallback;

                    // Mantenha uma referência ao delegate para evitar coleta de lixo
                    _callbackHandle = GCHandle.Alloc(callbackDelegate);

                    // Converta o delegate para um ponteiro de função
                    IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackDelegate);
                    MsQuic.ThrowIfFailure(ApiTable->ConnectionOpen(registration, (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>)callbackPtr.ToPointer(), ApiTable, &connection));

                    sbyte* google = stackalloc sbyte[50];
                    int written = Encoding.UTF8.GetBytes("google.com", new Span<byte>(google, 50));
                    google[written] = 0;

                    MsQuic.ThrowIfFailure(ApiTable->ConnectionStart(connection, configuration, 0, google, 443));
                    Thread.Sleep(1000);
                    _statusconnection.text = "Status: connected";
                }
                finally
                {
                    if (connection != null)
                    {
                        ApiTable->ConnectionShutdown(connection, QUIC_CONNECTION_SHUTDOWN_FLAGS.NONE, 0);
                        ApiTable->ConnectionClose(connection);
                    }
                    if (configuration != null)
                    {
                        ApiTable->ConfigurationClose(configuration);
                    }
                    if (registration != null)
                    {
                        ApiTable->RegistrationClose(registration);
                    }

                    // Libere o GCHandle
                    if (_callbackHandle.IsAllocated)
                    {
                        _callbackHandle.Free();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _statusconnection.text = $"Status: {ex.Message}";
            Debug.Log("Erro: " + ex.Message);
        }
    }

    public void DisconnectFromUnityQUIC()
    {
        _statusconnection.text = "Status: disconnected";
    }

    private static unsafe int NativeCallback(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt)
    {
        Debug.Log($"Connection event received: {evnt->Type}");
        if (evnt->Type == QUIC_CONNECTION_EVENT_TYPE.CONNECTED)
        {
            QUIC_API_TABLE* ApiTable = (QUIC_API_TABLE*)context;
            void* buf = stackalloc byte[128];
            uint len = 128;
            if (MsQuic.StatusSucceeded(ApiTable->GetParam(handle, MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, &len, buf)))
            {
                QuicAddr* addr = (QuicAddr*)(buf);
                Debug.Log($"Connected Family: {addr->Family}");
            }
        }
        if (evnt->Type == QUIC_CONNECTION_EVENT_TYPE.PEER_STREAM_STARTED)
        {
            Debug.Log("Aborting Stream");
            return MsQuic.QUIC_STATUS_ABORTED;
        }
        if (evnt->Type == QUIC_CONNECTION_EVENT_TYPE.SHUTDOWN_INITIATED_BY_TRANSPORT)
        {
            Debug.Log($"{evnt->SHUTDOWN_INITIATED_BY_TRANSPORT.Status.ToString("X8")}: {MsQuicException.GetErrorCodeForStatus(evnt->SHUTDOWN_INITIATED_BY_TRANSPORT.Status)}");
        }
        return MsQuic.QUIC_STATUS_SUCCESS;
    }

    public void Request()
    {
        _request.text = "Response: Request sent!";
    }
#endif
}
