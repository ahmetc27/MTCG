using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MTCG.Services;
using MTCG.Models;

namespace MTCG.Server
{
    public class HttpServer
    {
        private readonly UserService _userService = new();
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

            string[] requestLines = request.Split("\r\n");
            string[] requestParts = requestLines[0].Split(" ");
            if (requestParts.Length < 2)
            {
                client.Close();
                return;
            }

            string method = requestParts[0];  
            string path = requestParts[1];    

            string requestBody = requestLines.Length > 1 ? requestLines[requestLines.Length - 1] : "";

            string authHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization: "))?.Substring(15) ?? "";

            string response = RouteRequest(method, path, requestBody, authHeader);

            byte[] responseData = Encoding.UTF8.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
            client.Close();
        }

        private string RouteRequest(string method, string path, string requestBody, string authHeader)
        {
            if (method == "POST" && path == "/users")
            {
                return HandleUserRegistration(requestBody);
            }
            else if (method == "POST" && path == "/sessions")
            {
                return HandleUserLogin(requestBody);
            }
            else if (method == "GET" && path.StartsWith("/users/"))
            {
                return HandleGetUser(path, authHeader);
            }

            return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nRoute nicht gefunden";
        }
        private string HandleUserRegistration(string requestBody)
        {
            try
            {
                User? newUser = JsonSerializer.Deserialize<User>(requestBody);
                if (newUser == null || string.IsNullOrEmpty(newUser.Username) || string.IsNullOrEmpty(newUser.Password))
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nUngültige Eingabe";

                bool success = _userService.RegisterUser(newUser.Username, newUser.Password);
                if (success)
                    return "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nUser erfolgreich registriert";
                else
                    return "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\nUser existiert bereits";
            }
            catch
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nFehlerhafte JSON-Daten";
            }
        }

        private string HandleUserLogin(string requestBody)
        {
            try
            {
                User? loginUser = JsonSerializer.Deserialize<User>(requestBody);
                if (loginUser == null || string.IsNullOrEmpty(loginUser.Username) || string.IsNullOrEmpty(loginUser.Password))
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nUngültige Eingabe";

                string? token = _userService.AuthenticateUser(loginUser.Username, loginUser.Password);
                if (token == null)
                    return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nFalsche Login-Daten";

                return $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{token}";
            }
            catch
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nFehlerhafte JSON-Daten";
            }
        }

        private string HandleGetUser(string path, string authHeader)
        {
            string username = path.Substring(7); // "/users/Ahmet" → "Ahmet"

            if (string.IsNullOrEmpty(authHeader) || !_userService.ValidateToken(username, authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nZugriff verweigert";
            }

            User? user = _userService.GetUser(username);
            if (user == null)
            {
                return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser nicht gefunden";
            }

            string jsonResponse = JsonSerializer.Serialize(user);
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }
    }
}