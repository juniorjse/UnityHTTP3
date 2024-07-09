using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Quic;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt);

public class MsQuicUnity : MonoBehaviour
{
    public Text _text;

    public void ButtonFunction()
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
                    QUIC_BUFFER buffer = new();
                    buffer.Buffer = alpn;
                    buffer.Length = 2;
                    QUIC_SETTINGS settings = new();
                    settings.IsSetFlags = 0;
                    settings.IsSet.PeerBidiStreamCount = 1;
                    settings.PeerBidiStreamCount = 1;
                    settings.IsSet.PeerUnidiStreamCount = 1;
                    settings.PeerUnidiStreamCount = 3;
                    MsQuic.ThrowIfFailure(ApiTable->ConfigurationOpen(registration, &buffer, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, &configuration));
                    QUIC_CREDENTIAL_CONFIG config = new();
                    config.Flags = QUIC_CREDENTIAL_FLAGS.CLIENT;
                    MsQuic.ThrowIfFailure(ApiTable->ConfigurationLoadCredential(configuration, &config));

                    // Crie uma instância do delegate
                    NativeCallbackDelegate callbackDelegate = NativeCallback;

                    // Converta o delegate para um ponteiro de função
                    IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackDelegate);
                    MsQuic.ThrowIfFailure(ApiTable->ConnectionOpen(registration, (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>)callbackPtr.ToPointer(), ApiTable, &connection));

                    sbyte* google = stackalloc sbyte[50];
                    int written = Encoding.UTF8.GetBytes("google.com", new Span<byte>(google, 50));
                    google[written] = 0;
                    MsQuic.ThrowIfFailure(ApiTable->ConnectionStart(connection, configuration, 0, google, 443));
                    Thread.Sleep(1000);
                    _text.text = "Conexão bem sucedida!";
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
                    MsQuic.Close(ApiTable);
                }
            }
        }
        catch (Exception ex)
        {
            _text.text = $"Erro: {ex.Message}";
            Debug.Log("Erro: " + ex.Message);
        }
    }

    private static unsafe int NativeCallback(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt)
    {
        Debug.Log("Evento de conexão recebido");
        // Aqui você pode adicionar mais lógica para lidar com os eventos de conexão
        return 0; // Retorne um valor apropriado baseado no evento
    }


}
