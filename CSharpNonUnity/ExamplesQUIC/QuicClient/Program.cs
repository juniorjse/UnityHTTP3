using System;
using System.Text;
using QuicNet;
using QuicNet.Streams;
using QuicNet.Connections;

namespace QuicClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                QuicClient client = new QuicClient();
                Console.WriteLine("Client initialized.");

                QuicConnection connection = client.Connect("127.0.0.1", 11001);
                Console.WriteLine("Client connected.\n");

                
                // Create a data stream
                QuicStream stream = connection.CreateStream(QuickNet.Utilities.StreamType.ClientBidirectional);
                // Send Data
                stream.Send(Encoding.UTF8.GetBytes("Hello from Client!"));
                // Wait reponse back from the server (Blocks)
                byte[] data = stream.Receive();
                Console.WriteLine("Received back: "+Encoding.UTF8.GetString(data));

                // Create a new data stream
                stream = connection.CreateStream(QuickNet.Utilities.StreamType.ClientBidirectional);
                // Send Data
                stream.Send(Encoding.UTF8.GetBytes("Hello from Client2!"));
                // Wait reponse back from the server (Blocks)
                data = stream.Receive();

                Console.WriteLine("Received back: "+Encoding.UTF8.GetString(data));

                Console.ReadKey();

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}