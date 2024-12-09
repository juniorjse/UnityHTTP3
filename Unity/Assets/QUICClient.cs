using System.Net.Http;
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Quic;
using System.Threading;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt);
public class QUICClient : MonoBehaviour
{
    public Text _statusconnection;
    public Text _request;
    private GCHandle _callbackHandle;
    private unsafe QUIC_HANDLE* connection = null;
    private unsafe QUIC_HANDLE* registration = null;
    private unsafe QUIC_HANDLE* configuration = null;
    private unsafe QUIC_API_TABLE* ApiTable;

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
                    IsSetFlags = 0
                };
                settings.IsSet.PeerBidiStreamCount = 1;
                settings.PeerBidiStreamCount = 1;
                settings.IsSet.PeerUnidiStreamCount = 1;
                settings.PeerUnidiStreamCount = 3;

                // Fixando o ponteiro para configuração
                fixed (QUIC_HANDLE** pConfiguration = &configuration)
                {
                    MsQuic.ThrowIfFailure(ApiTable->ConfigurationOpen(registration, &buffer, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, pConfiguration));
                }

                QUIC_CREDENTIAL_CONFIG config = new()
                {
                    Flags = QUIC_CREDENTIAL_FLAGS.CLIENT
                };
                MsQuic.ThrowIfFailure(ApiTable->ConfigurationLoadCredential(configuration, &config));

                // Callback
                NativeCallbackDelegate callbackDelegate = NativeCallback;
                _callbackHandle = GCHandle.Alloc(callbackDelegate);
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

                // Fixando o ponteiro para conexão
                fixed (QUIC_HANDLE** pConnection = &connection)
                {
                    MsQuic.ThrowIfFailure(ApiTable->ConnectionOpen(registration, (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>)callbackPtr.ToPointer(), null, pConnection));
                }

                // Inicia a conexão
                sbyte* google = stackalloc sbyte[50];
                int written = Encoding.UTF8.GetBytes("google.com", new Span<byte>(google, 50));
                google[written] = 0;

                MsQuic.ThrowIfFailure(ApiTable->ConnectionStart(connection, configuration, 0, google, 443));
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

    private static unsafe int NativeCallback(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt)
    {
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

    public async void Request()
    {
        string url = "https://www.google.com/search?q=WildlifeStudios&tbm=nws";
        Debug.Log($"🌐 GET request sent to {url}");
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _request.text = $"Request failed: {response.StatusCode}";
                    Debug.Log($"Request failed: {response.StatusCode}");
                    return;
                }

                string htmlString = await response.Content.ReadAsStringAsync();

                string truncatedResponse = htmlString.Length > 300 ? htmlString.Substring(0, 300) + "..." : htmlString;

                _request.text = $"Response: {truncatedResponse}";
                Debug.Log($"Response: {truncatedResponse}");
            }
        }
        catch (HttpRequestException ex)
        {
            _request.text = $"Request failed: {ex.Message}";
            Debug.Log($"Request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _request.text = $"Unexpected error: {ex.Message}";
            Debug.Log($"Unexpected error: {ex.Message}");
        }
    }
#endif
}
