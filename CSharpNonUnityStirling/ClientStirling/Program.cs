using System;
using System.Reflection;
using StirlingLabs.MsQuic;
using StirlingLabs.Utilities;

public class Program
{
    private static QuicClientConnection _clientSide = null!;

    public static void Main()
    {
        QuicRegistration registration = new QuicRegistration("ClientTest");
        PrintObjectProperties(registration, "QuicRegistration");
        Console.WriteLine("Registrado!\n");
        bool reliableDatagrams = false;
        SizedUtf8String[] alpns = new SizedUtf8String[] { SizedUtf8String.Create("sample") };

        QuicClientConfiguration config = new QuicClientConfiguration(registration, reliableDatagrams, alpns);
        PrintObjectProperties(config, "QuicClientConfiguration");
        Console.WriteLine("Configurado!\n");

        _clientSide = new QuicClientConnection(config);
        PrintObjectProperties(_clientSide, "QuicClientConnection");
        Console.WriteLine("Conexão criada!\n");

        ushort port = 11001;
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

    public static void PrintObjectProperties(object obj, string objName)
    {
        Console.WriteLine($"Propriedades do objeto {objName}:");
        PropertyInfo[] properties = obj.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            object value = property.GetValue(obj);
            if (value == null)
            {
                Console.WriteLine($"{property.Name}: NULL");
            }
        }
    }
}