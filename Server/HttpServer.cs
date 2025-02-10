using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server
{
    public class HttpServer
    {
        private readonly int _port;
        private readonly TcpListener _listener;

        public HttpServer(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine($"Server läuft auf Port {_port}...");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Task.Run(() => HandleClient(client)); // Client in eigenem Thread verarbeiten
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Anfrage erhalten:\n{request}");

            // HTTP-Response bauen
            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, MTCG!";
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);

            client.Close();
        }
    }
}