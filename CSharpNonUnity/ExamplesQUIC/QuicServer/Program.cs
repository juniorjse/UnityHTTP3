using System;
using System.Text;
using QuicNet;
using QuicNet.Streams;
using QuicNet.Connections;

namespace QuickNet.Tests.ConsoleServer
{
    class Program
    {
        // Fired when a client is connected
        static void ClientConnected(QuicConnection connection)
        {
            connection.OnStreamOpened += StreamOpened;
            Console.WriteLine("Client connected.");
        }

        // Fired when a new stream has been opened (It does not carry data with it)
        static void StreamOpened(QuicStream stream)
        {
            stream.OnStreamDataReceived += StreamDataReceived;
        }

        // Fired when a stream received full batch of data
        static void StreamDataReceived(QuicStream stream, byte[] data)
        {
            string decoded = Encoding.UTF8.GetString(data);
            Console.WriteLine($"Received: {decoded}");

            // Send back data to the client on the same stream
            stream.Send(Encoding.UTF8.GetBytes("Ping back from server."));
        }

        static void Main(string[] args)
        {
            QuicListener listener = new QuicListener(11001);
            Console.WriteLine("QUIC Server started on port 11001");
            listener.OnClientConnected += ClientConnected;

            listener.Start();

            Console.ReadKey();
        }
    }
}