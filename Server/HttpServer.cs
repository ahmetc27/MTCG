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

            // Die erste Zeile der HTTP-Anfrage auslesen (z. B. "GET /users HTTP/1.1")
            string[] requestLines = request.Split("\r\n");
            string[] requestParts = requestLines[0].Split(" ");
            if (requestParts.Length < 2)
            {
                client.Close();
                return;
            }

            string method = requestParts[0];  // GET, POST, PUT, DELETE
            string path = requestParts[1];    // /users, /cards, etc.

            string response = RouteRequest(method, path);

            byte[] responseData = Encoding.UTF8.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
            client.Close();
        }

        private string RouteRequest(string method, string path)
        {
            if (method == "GET" && path == "/users")
            {
                return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nListe der User";
            }
            else if (method == "POST" && path == "/users")
            {
                return "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nUser erstellt";
            }
            else if (method == "GET" && path == "/cards")
            {
                return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nListe der Karten";
            }
            return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nRoute nicht gefunden";
        }

    }
}