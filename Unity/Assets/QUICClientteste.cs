// using System;
// using System.Runtime.InteropServices;
// using UnityEngine;
// using UnityEngine.UI;

// [AttributeUsage(AttributeTargets.Method)]
// public class MonoPInvokeCallbackAttribute : Attribute
// {
//     public Type DelegateType { get; }

//     public MonoPInvokeCallbackAttribute(Type type)
//     {
//         DelegateType = type;
//     }
// }

// public class QUICClientteste : MonoBehaviour
// {
//     public Text _statusconnection;
//     public Text _request;
//     public delegate void StringCallback(string arg);
//     private static QUICClientteste instance;
//     private Action<string> currentCallback;

//     private void Awake()
//     {
//         instance = this;
//     }

//     [MonoPInvokeCallback(typeof(StringCallback))]
//     static void HandleResult(string arg)
//     {
//         try
//         {
//             Debug.Log("HandleResult received: " + arg);

//             if (instance == null)
//             {
//                 Debug.LogError("Instance of QUICClientteste is null. Ensure the script is attached and initialized.");
//                 return;
//             }

//             if (instance.currentCallback != null)
//             {
//                 instance.currentCallback(arg);
//             }
//             else
//             {
//                 Debug.LogError("currentCallback is not set in HandleResult. Ensure a callback is assigned before calling native methods.");
//             }
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError("Error in HandleResult: " + ex.Message);
//         }
//     }

//     private void UpdateStatus(string arg)
//     {
//         Debug.Log("Status result: " + arg);

//         if (_statusconnection != null)
//         {
//             _statusconnection.text = "Status: " + arg;
//         }
//         else
//         {
//             Debug.LogError("The _statusconnection field is null.");
//         }
//     }

//     private void UpdateRequest(string arg)
//     {
//         Debug.Log("Request result: " + arg);

//         if (_request != null)
//         {
//             _request.text = "Response: " + arg;
//         }
//         else
//         {
//             Debug.LogError("The _request field is null.");
//         }
//     }

//     public void ConnectVerify()
//     {
// #if UNITY_IOS
//         currentCallback = UpdateStatus;
//         connectToQUIC(HandleResult);
// #else
//         Debug.Log("ConnectVerify called, but not on iOS.");
// #endif
//     }

//     public void DisconnectVerify()
//     {
// #if UNITY_IOS
//         string result = Marshal.PtrToStringAnsi(disconnectFromQUIC());
//         UpdateStatus(result);
// #else
//         Debug.Log("DisconnectVerify called, but not on iOS.");
// #endif
//     }

//     public void RequestVerify()
//     {
// #if UNITY_IOS
//         currentCallback = UpdateRequest;
//         getRequestToServer(HandleResult);
// #else
//         Debug.Log("RequestVerify called, but not on iOS.");
// #endif
//     }

// #if UNITY_IOS
//     [DllImport("__Internal")]
//     private static extern void connectToQUIC(StringCallback completionHandler);

//     [DllImport("__Internal")]
//     private static extern IntPtr disconnectFromQUIC();

//     [DllImport("__Internal")]
//     private static extern void getRequestToServer(StringCallback completionHandler);
// #endif
// }
