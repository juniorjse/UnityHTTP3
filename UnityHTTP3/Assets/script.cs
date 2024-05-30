using System;
using UnityEngine;
using UnityEngine.UI;
using StirlingLabs.MsQuic;
using StirlingLabs.Utilities;
using System.Collections;

public class script : MonoBehaviour
{
    private QuicClientConnection _clientSide;
    public Text _text;

    void Start()
    {
        try
        {
            Debug.Log("Criando registro...");
            QuicRegistration registration = new("UnityClient");
            Debug.Log("Registro criado.");

            bool reliableDatagrams = true;
            SizedUtf8String[] alpns = new SizedUtf8String[] { SizedUtf8String.Create("h3-29") };

            Debug.Log("Criando configuração...");
            QuicClientConfiguration config = new(registration, reliableDatagrams, alpns);
            Debug.Log("Configuração criada.");

            Debug.Log("Criando conexão do cliente...");
            _clientSide = new QuicClientConnection(config);
            Debug.Log("Conexão do cliente criada.");
        }
        catch (Exception ex)
        {
            Debug.Log($"Erro ao iniciar: {ex.Message}");
        }
    }

    public void ButtonFunction()
    {
        ConnectAsync(4244); // Substitua com a porta correta
    }

    public void ButtonFunction2()
    {
        ConnectAsync(443); // Substitua com a porta correta
    }

    private void ConnectAsync(ushort port)
    {
        StartCoroutine(ConnectCoroutine(port));
    }

    private IEnumerator ConnectCoroutine(ushort port)
    {
        Debug.Log("Tentando conectar...");
        var connectTask = _clientSide.ConnectAsync(SizedUtf8String.Create("127.0.0.1"), port); // Substitua com o IP correto (www.google.com, pode ser  usado para testar)

        while (!connectTask.IsCompleted)
        {
            yield return null;
        }

        if (connectTask.Exception != null)
        {
            _text.text = $"Erro ao conectar: {connectTask.Exception.Message}";
            Debug.Log($"Erro ao conectar: {connectTask.Exception.Message}");
        }
        else
        {
            _text.text = "Conexão bem sucedida!";
            Debug.Log("Conexão bem sucedida!");
        }
    }
}