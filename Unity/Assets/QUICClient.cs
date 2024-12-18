using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Quic;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* evnt);

public class QUICClient : MonoBehaviour
{
    public Text _statusconnection;
    public Text _request;
    private GCHandle _callbackHandle;
    // Definir a DLL do msquic
    private const string MsquicDll = "msquic.dll";

    // Função de P/Invoke para abrir a biblioteca msquic
    [DllImport(MsquicDll, SetLastError = true)]
    private static extern int MsQuicOpen(IntPtr config, out IntPtr handle);

    // Função para fechar a conexão QUIC
    [DllImport(MsquicDll, SetLastError = true)]
    private static extern int MsQuicClose(IntPtr handle);

    // Função para iniciar a negociação de handshake QUIC
    [DllImport(MsquicDll, SetLastError = true)]
    private static extern int MsQuicStart(IntPtr handle);

    // Função para enviar dados QUIC
    [DllImport(MsquicDll, SetLastError = true)]
    private static extern int MsQuicSend(IntPtr handle, byte[] buffer, int length);

    // Função para receber dados QUIC
    [DllImport(MsquicDll, SetLastError = true)]
    private static extern int MsQuicRecv(IntPtr handle, byte[] buffer, int length);

    public void Teste()
    {

    
        IntPtr msquicHandle = IntPtr.Zero;
        try
        {
            // Abrir a biblioteca msquic
            int result = MsQuicOpen(IntPtr.Zero, out msquicHandle);
            if (result != 0 || msquicHandle == IntPtr.Zero)
            {
                Console.WriteLine("Erro ao abrir msquic.");
                return;
            }

            Debug.Log("msquic aberto com sucesso.");

            // Iniciar a negociação QUIC
            result = MsQuicStart(msquicHandle);
            if (result != 0)
            {
                Debug.Log("Erro ao iniciar QUIC.");
                return;
            }

            Debug.Log("HandShake QUIC iniciado.");

            // Enviar dados para o servidor
            var dataToSend = System.Text.Encoding.UTF8.GetBytes("Exemplo de dados QUIC");
            result = MsQuicSend(msquicHandle, dataToSend, dataToSend.Length);
            if (result != 0)
            {
                Debug.Log("Erro ao enviar dados.");
                return;
            }

            Debug.Log("Dados enviados com sucesso.");

            // Receber dados do servidor
            byte[] buffer = new byte[1024];
            result = MsQuicRecv(msquicHandle, buffer, buffer.Length);
            if (result != 0)
            {
                Debug.Log("Erro ao receber dados.");
                return;
            }

            Debug.Log("Dados recebidos: " + System.Text.Encoding.UTF8.GetString(buffer));

        }
        catch (Exception ex)
        {
            Debug.Log("Erro geral: " + ex.Message);
        }
        finally
        {
            // Fechar a conexão
            if (msquicHandle != IntPtr.Zero)
            {
                MsQuicClose(msquicHandle);
                Debug.Log("Conexão QUIC fechada.");
            }
        }
    }
}





          

