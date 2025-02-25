using System;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Transport;

public class QUICClient : MonoBehaviour
{
    [Header("Connection Settings")]
    public string host = "localhost";
    public ushort port = 3250;
    public IPitayaClient client;
    private string handshakeOpts = "h3";

    [Header("Request Settings")]
    public string route = "connector.getsessiondata";

    private int timeout = 60;
    public TextMeshProUGUI _statusconnection;
    public TextMeshProUGUI _request;

    private static QUICClient instance;

    private void Awake()
    {
        instance = this;
        client = new PitayaClient(TransporterName.QUIC);
    }

    /// <summary>
    /// Atualiza o texto de status na tela.
    /// </summary>
    /// <param name="arg">Texto a ser exibido.</param>
    private void UpdateStatus(string arg)
    {
        Debug.Log("Status result: " + arg);
        if (_statusconnection != null)
            _statusconnection.text = "Status: " + arg;
    }

    /// <summary>
    /// Atualiza o texto de request na tela.
    /// </summary>
    /// <param name="arg">Texto a ser exibido.</param>
    private void UpdateRequest(string arg)
    {
        Debug.Log("Request result: " + arg);
        if (_request != null)
            _request.text = "Request: " + arg;
    }

    public void ConnectVerify()
    {
#if UNITY_ANDROID
        // client.Connect(host, port, handshakeOpts);
        AndroidConnection();
#else
        Debug.Log("ConnectVerify chamado, mas não é Android.");
#endif
    }

    public void RequestVerify()
    {
#if UNITY_ANDROID
        // client.Request(route,
        // res =>
        // {
        //     PitayaClientLib.Logger.LogDebug($"[{route}] - response={res}");
        // },
        // error =>
        // {
        //     PitayaClientLib.Logger.LogDebug($"[{route}] ERROR - error-code={error.Code} metadata={error.Metadata}");
        // });
        AndroidRequest();
#else
        Debug.Log("RequestVerify chamado, mas não é Android.");
#endif
    }

    public void DisconnectVerify()
    {
#if UNITY_ANDROID
        // client.Disconnect();
        AndroidDisconnect();
#else
        Debug.Log("DisconnectVerify chamado, mas não é Android.");
#endif
    }

    /// <summary>
    /// Método de conexão via Android (chama plugin Java).
    /// </summary>
    public void AndroidConnection()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject pluginObject = new AndroidJavaObject("com.example.quicconnectionwrapper.QuicInstructions");

        string result = pluginObject.Call<string>("QuicAndroidConnect", currentActivity);
        UpdateStatus(result);
    }

    /// <summary>
    /// Método de requisição via Android (chama plugin Java).
    /// </summary>
    public void AndroidRequest()
    {
        AndroidJavaObject pluginObject = new AndroidJavaObject("com.example.quicconnectionwrapper.QuicInstructions");
        string requestResult = pluginObject.Call<string>("QuicAndroidGet");

        // Desabilita Rich Text
        _request.richText = false;
        // Atualiza o texto
        UpdateRequest(requestResult);
    }


    /// <summary>
    /// Método de desconexão via Android (chama plugin Java).
    /// </summary>
    public void AndroidDisconnect()
    {
        AndroidJavaObject pluginObject = new AndroidJavaObject("com.example.quicconnectionwrapper.QuicInstructions");
        pluginObject.Call("QuicAndroidDisconnect");
        UpdateStatus("DESCONECTADO");
    }
}
