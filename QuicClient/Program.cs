using System;
using System.Text;
using QuicNet;
using QuicNet.Connections;
using QuicNet.Streams;

namespace QuicClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initializing client...");
                QuicClient client = new QuicClient();

                Console.WriteLine("Attempting to connect to server...");
                QuicConnection connection = client.Connect("127.0.0.1", 11000);

                Console.WriteLine("Connected to the server.");

                Console.WriteLine("Creating stream...");
                QuicStream stream = connection.CreateStream(QuickNet.Utilities.StreamType.ClientBidirectional);

                Console.WriteLine("Sending data...");
                stream.Send(Encoding.UTF8.GetBytes("Hello from .NET Client!"));

                Console.WriteLine("Receiving data...");
                byte[] data = stream.Receive();

                string response = Encoding.UTF8.GetString(data);
                Console.WriteLine($"Data received: {response}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
