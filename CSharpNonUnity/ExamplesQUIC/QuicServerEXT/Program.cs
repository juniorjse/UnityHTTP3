using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDPListener
{
    private const int listenPort = 11001;
    private static int messageCounter = 1;

    private static void StartListener()
    {
        UdpClient listener = new UdpClient(listenPort);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

        Console.WriteLine("Server started on port 11001\n");

        try
        {
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                string message = Encoding.UTF8.GetString(bytes);

                Console.WriteLine($"Received {messageCounter}: {message}, Size: {bytes.Length} bytes, Type: {Encoding.UTF8.EncodingName}.");

                // Echo back the message to the client using a stream
                using (MemoryStream memStream = new MemoryStream(bytes))
                {
                    listener.Send(memStream.ToArray(), memStream.ToArray().Length, groupEP);
                }
                messageCounter++;
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
