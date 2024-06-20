using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class SimpleHttpServer
{
    public static async Task Main(string[] args)
    {
        string url = "http://127.0.0.1:8081/";

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        Console.WriteLine("Servidor HTTP iniciado. Aguardando conexões...");

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string responseString = "<html><body><h1>Servidor HTTP Simples</h1><p>Olá, mundo!</p></body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
