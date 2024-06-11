using System;
using QuicNet;
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
                Console.WriteLine("Client connected: " + connection); // QuicNet.Connections.QuicConnection ele fica aguardando um objeto QuicConnection por  isso nao finaliza ao se conectar com um outro servidor UDP.

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}


/*
  public QuicConnection Connect(string ip, int port)
    {
        _peerIp = new IPEndPoint(IPAddress.Parse(ip), port);
        _pwt = new PacketWireTransfer(_client, _peerIp);
        InitialPacket packet = _packetCreator.CreateInitialPacket(0uL, 0uL);
        _pwt.SendPacket(packet);
        InitialPacket initialPacket = (InitialPacket)_pwt.ReadPacket();
        HandleInitialFrames(initialPacket);
        EstablishConnection(initialPacket.SourceConnectionId, initialPacket.SourceConnectionId);
        return _connection;
    }
*/