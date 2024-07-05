using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDPListener
{
    private const int listenPort = 11001;

    private static void StartListener()
    {
        UdpClient listener = new UdpClient(listenPort);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for broadcast");
                byte[] bytes = listener.Receive(ref groupEP);

                //string receivedMessage = Encoding.Unicode.GetString(bytes);
                string receivedMessage = BitConverter.ToString(bytes);
                int length = receivedMessage.Length;
                Console.WriteLine($"Received broadcast from {groupEP} : {length}");

                // Enviar a mensagem "oi" de volta para o cliente
                byte[] responseBytes = Encoding.UTF8.GetBytes("oi");
                Console.WriteLine($"Sending response to {groupEP}");
                listener.Send(responseBytes, responseBytes.Length, groupEP);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            listener.Close();
        }
    }

    public static void Main()
    {
        StartListener();
    }
}
