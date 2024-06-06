using System;
using System.Text;
using QuicNet;
using QuicNet.Connections;
using QuicNet.Streams;
using UnityEngine;
using UnityEngine.UI;

public class script : MonoBehaviour
{
    private QuicClient client;
    private QuicConnection connection;

    public Text _text;

    private void Start()
    {
        InitializeClient();
    }

    private void InitializeClient()
    {
        try
        {
            client = new QuicClient();

            connection = client.Connect("127.0.0.1", 11000);

            Debug.Log("Connected to the server.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error connecting: {ex.Message}");
        }
    }

    public string ConnectToQuic()
    {
        try
        {
            QuicStream stream = connection.CreateStream(QuickNet.Utilities.StreamType.ClientBidirectional);

            stream.Send(Encoding.UTF8.GetBytes("Hello from UnityClient!"));

            byte[] data = stream.Receive();

            return Encoding.UTF8.GetString(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending/receiving data: {ex.Message}");
            return "Communication error with the server.";
        }
    }

    public void ButtonFunction()
    {
        try
        {
            string response = ConnectToQuic();
            _text.text = response;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in ButtonFunction: {ex.Message}");
        }
    }

    private static string GenerateData(int kb)
    {
        StringBuilder res = new StringBuilder();
        for (int i = 0; i < kb; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                res.Append("!!!!!!!!!!");
            }
        }

        return res.ToString();
    }

    private static string GenerateBytes(int bytes)
    {
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < bytes; i++)
        {
            result.Append("!");
        }

        return result.ToString();
    }
}
