using System;
using System.Net;

namespace SimpleHttpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Se eu inicializar o servidor do exemplo nessa mesma porta 11001 nao dá erro de uso. como se a porta estivesse livre. Porém, no Listener do exemplo ele nao utiliza o addrs, apenas a porta.
                var request = WebRequest.Create("127.0.0.1:11001");
                using (var response = request.GetResponse())
                {
                    Console.WriteLine("Conexão bem-sucedida com o servidor.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro ao se conectar ao servidor: {ex.Message}");
            }
        }
    }
}
