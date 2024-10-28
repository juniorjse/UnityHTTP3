using System;
using System.Reflection;
using StirlingLabs.MsQuic;
using StirlingLabs.Utilities;

public class Program
{
    private static QuicClientConnection? connection;
    private static QuicClientConfiguration? configuration;
    private static QuicRegistration? registration;

    public static void Main()
    {
        try
        {
            registration = new QuicRegistration("ClientTest");
            PrintObjectProperties(registration, "QuicRegistration");
            Console.WriteLine("Registrado!\n");

            bool reliableDatagrams = false;
            SizedUtf8String[] alpns = new SizedUtf8String[] { SizedUtf8String.Create("sample") };

            configuration = new QuicClientConfiguration(registration, reliableDatagrams, alpns);
            PrintObjectProperties(configuration, "QuicClientConfiguration");
            Console.WriteLine("Configurado!\n");

            connection = new QuicClientConnection(configuration);
            PrintObjectProperties(connection, "QuicClientConnection");
            Console.WriteLine("Conexão criada!\n");

            ushort port = 11001;
            try
            {
                connection.ConnectAsync(SizedUtf8String.Create("127.0.0.1"), port).Wait();
                Console.WriteLine("Conexão bem sucedida!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na inicialização: {ex.Message}");
        }
    }

    public static void PrintObjectProperties(object obj, string objName)
    {
        Console.WriteLine($"Propriedades do objeto {objName}:");
        PropertyInfo[] properties = obj.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            try
            {
                object? value = property.GetValue(obj);
                Console.WriteLine($"{property.Name}: {(value ?? "NULL")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{property.Name}: Erro ao obter valor ({ex.Message})");
            }
        }
    }
}
