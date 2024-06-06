using System;
using UnityEngine;
using UnityEngine.UI;
using StirlingLabs.MsQuic;
using StirlingLabs.Utilities;

public class script : MonoBehaviour
{
    private QuicClientConnection _clientSide = null!;

    public Text _text;

    void Start()
    {
        QuicRegistration registration = new QuicRegistration("ClientTest");
        bool reliableDatagrams = false;
        SizedUtf8String[] alpns = new SizedUtf8String[] { SizedUtf8String.Create("h1") };

        QuicClientConfiguration config = new QuicClientConfiguration(registration, reliableDatagrams, alpns);

        _clientSide = new QuicClientConnection(config);
    }

    public async void ButtonFunction()
    {
        ushort port = 443;
        try
        {
            await _clientSide.ConnectAsync(SizedUtf8String.Create("localhost"), port);
            _text.text = "Conexão bem sucedida!";
        }
        catch (Exception ex)
        {
            _text.text = $"Erro ao conectar: {ex.Message}";
        }
    }

}