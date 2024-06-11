using System;
using QuicNet;
using QuicNet.Connections;

namespace QuicServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //O servidor atual utiliza no Listerner apenas a porta, porém, o servidor externo utiliza adrrs e porta. Caso inicie ambos servidores na mesma porta nao ocorre erro de uso.
            //Isso acima ocorre em servidor TCP, porém, se eu tentar iniciar um servidor UDP na mesma porta ocorre erro de uso.
            QuicListener listener = new QuicListener(11001);
            Console.WriteLine("Server started.");
            listener.Start();
        }
    }
}

//Um client que usa UDP talvez nao conisga se conectar com servidor TLS.