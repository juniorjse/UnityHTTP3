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
    public Text _statusconnection;
    public Text _request;

    public delegate void StringCallback(string arg);

    private static QUICClientteste instance;

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
            
            if (instance != null)
            {
                instance.doSomething(arg);
            }
            else
            {
                Debug.LogError("Instance of QUICClientteste is null in HandleResult.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in HandleResult: " + ex.Message);
        }
    }

    private void doSomething(string arg)
    {
        Debug.Log("Connection result: " + arg);

        if (_statusconnection != null)
        {
            _statusconnection.text = "Status: " + arg;
        }
        else
        {
            Debug.LogError("The _statusconnection field is null.");
        }
    }

    public void ConnectVerify()
    {
#if UNITY_IOS
        connectToQUIC(HandleResult);
#else
        Debug.Log("ConnectVerify called, but not on iOS.");
#endif
    }

    public void DisconnectVerify()
    {
#if UNITY_IOS
        string result = Marshal.PtrToStringAnsi(disconnectFromQUIC());
        _statusconnection.text = "Status: " + result;
#else
        Debug.Log("DisconnectVerify called, but not on iOS.");
#endif
    }

    public void RequestVerify()
    {
#if UNITY_IOS
        IntPtr resultPtr = getRequest();
        string result = Marshal.PtrToStringAnsi(resultPtr);
        Marshal.FreeHGlobal(resultPtr);

        _request.text = "Response: " + result;
#else
        Debug.Log("RequestVerify called, but not on iOS.");
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void connectToQUIC(StringCallback completionHandler);

    [DllImport("__Internal")]
    private static extern IntPtr disconnectFromQUIC();

    [DllImport("__Internal")]
    private static extern IntPtr getRequest();
#endif
}
