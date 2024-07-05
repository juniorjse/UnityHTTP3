#nullable enable
using System;
using StirlingLabs.MsQuic;
using StirlingLabs.Utilities;
using System.Reflection;
using System.Threading.Tasks;

public class Program
{
    private static QuicClientConnection? connection;
    private static QuicClientConfiguration? configuration;
    private static QuicRegistration? registration;

    public static async Task Main()
    {
        try
        {
            // Step 1: Registration
            registration = new QuicRegistration("ClientTest");
            PrintObjectProperties(registration, "QuicRegistration");
            Console.WriteLine("Registrado!\n");

            // Step 2: Configuration
            bool reliableDatagrams = false;
            SizedUtf8String[] alpns = new SizedUtf8String[] { SizedUtf8String.Create("h3") }; // Ajuste ALPN para "h3"

            configuration = new QuicClientConfiguration(registration, reliableDatagrams, alpns);
            PrintObjectProperties(configuration, "QuicClientConfiguration");
            Console.WriteLine("Configurado!\n");

            // Step 3: Connection
            connection = new QuicClientConnection(configuration);
            PrintObjectProperties(connection, "QuicClientConnection");
            Console.WriteLine("Conexão criada!\n");

            // Step 4: Connect
            ushort port = 443; // Porta padrão para HTTPS
            try
            {
                var serverAddress = SizedUtf8String.Create("google.com");
                Console.WriteLine($"Tentando conectar ao servidor {serverAddress} na porta {port}...");

                // Verificar se o endereço está correto
                if (serverAddress.Length == 0)
                {
                    throw new ArgumentException("O endereço do servidor não pode estar vazio.");
                }

                await connection.ConnectAsync(serverAddress, port);
                Console.WriteLine("Conexão bem sucedida!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"::ERROR:: connection - {ex.Message}\n");
            }

            // Reimprimir propriedades após tentativa de conexão
            PrintObjectProperties(connection, "QuicClientConnection Pós-Conexão");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na inicialização: {ex.Message}");
        }
        finally
        {
            // Step 5: Cleanup
            connection?.Dispose();
            configuration?.Dispose();
            registration?.Dispose();
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
#nullable disable
