using System;
using StirlingLabs.MsQuic;
using StirlingLabs.Utilities;

public class Program
{
    private static QuicClientConnection _clientSide = null!;

    public static void Main()
    {
        QuicRegistration registration = new QuicRegistration("ClientTest");
        bool reliableDatagrams = false;
        SizedUtf8String[] alpns = new SizedUtf8String[] { SizedUtf8String.Create("h1") };

        QuicClientConfiguration config = new QuicClientConfiguration(registration, reliableDatagrams, alpns);

        _clientSide = new QuicClientConnection(config);

        ushort port = 443;
        try
        {
            _clientSide.ConnectAsync(SizedUtf8String.Create("127.0.0.1"), port).Wait();
            Console.WriteLine("Conexão bem sucedida!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
        }
    }
}
