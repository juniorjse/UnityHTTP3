using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

[AttributeUsage(AttributeTargets.Method)]
public class MonoPInvokeCallbackAttribute : Attribute
{
    public Type DelegateType { get; }

    public MonoPInvokeCallbackAttribute(Type type)
    {
        DelegateType = type;
    }
}

public class QUICClientteste : MonoBehaviour
{
    [Header("Connection Settings")]
    public string host = "www.google.com";
    public ushort port = 443;
    private string handshakeOpts = "";

    [Header("Request Settings")]
    public string route = "/search?q=WildlifeStudios&tbm=nws";
    private int messageType = 1;
    private uint sequenceNumber = 1;
    private IntPtr data = IntPtr.Zero;
    private uint requestUid = 1;
    private int timeout = 60;  
    public Text _statusconnection;
    public Text _request;
    public delegate void StringCallback(string arg);
    private static QUICClientteste instance;
    private Action<string> currentCallback;

    private void Awake()
    {
        instance = this;
    }

    [MonoPInvokeCallback(typeof(StringCallback))]
    static void HandleResult(string arg)
    {
        try
        {
            Debug.Log("HandleResult received: " + arg);

            if (instance == null)
            {
                Debug.LogError("Instance of QUICClientteste is null. Ensure the script is attached and initialized.");
                return;
            }

            if (instance.currentCallback != null)
            {
                instance.currentCallback(arg);
            }
            else
            {
                Debug.LogError("currentCallback is not set in HandleResult. Ensure a callback is assigned before calling native methods.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in HandleResult: " + ex.Message);
        }
    }

    private void UpdateStatus(string arg)
    {
        Debug.Log("Status result: " + arg);

        if (_statusconnection != null)
        {
            _statusconnection.text = "Status: " + arg;
        }
        else
        {
            Debug.LogError("The _statusconnection field is null.");
        }
    }

    private void UpdateRequest(string arg)
    {
        Debug.Log("Request result: " + arg);

        if (_request != null)
        {
            _request.text = "Response: " + arg;
        }
        else
        {
            Debug.LogError("The _request field is null.");
        }
    }

    public void ConnectVerify()
    {
#if UNITY_IOS
        currentCallback = UpdateStatus;
        connectQUIC(host, port, handshakeOpts, HandleResult);
#else
        Debug.Log("ConnectVerify called, but not on iOS.");
#endif
    }

    public void RequestVerify()
    {
#if UNITY_IOS
        currentCallback = UpdateRequest;
        sendQUIC(route, messageType, sequenceNumber, data, requestUid, timeout, HandleResult);
#else
        Debug.Log("RequestVerify called, but not on iOS.");
#endif
    }

    public void DisconnectVerify()
    {
#if UNITY_IOS
        string result = Marshal.PtrToStringAnsi(disconnect());
        UpdateStatus(result);
#else
        Debug.Log("DisconnectVerify called, but not on iOS.");
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void connectQUIC(string host, ushort port, string handshakeOpts, StringCallback completionHandler);

    [DllImport("__Internal")]
    private static extern void sendQUIC(string route, int messageType, uint sequenceNumber, IntPtr data, uint requestUid, int timeout, StringCallback completionHandler);
    [DllImport("__Internal")]
    private static extern IntPtr disconnect();
#endif
}
