using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MTCG.Services;
using MTCG.Models;
using MTCG.Repositories;
using Npgsql;

namespace MTCG.Server
{
    public class HttpServer
    {
        private readonly UserService _userService;
        private readonly CardService _cardService = new(); // Neue Instanz für Karten-Service
        private readonly DeckService _deckService = new();
        private readonly TradingService _tradingService = new();
        private readonly BattleService _battleService;

        private readonly int _port;
        private readonly TcpListener _listener;

       public HttpServer(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
            _userService = new UserService(_cardService);
            _deckService = new DeckService();
            _battleService = new BattleService(_deckService, _userService);

            _tradingService.AddTrade(new Trade("trade1", "Ahmet", new Card("1", "FireDragon", 50, "Monster"), "Monster", 30));
            _tradingService.AddTrade(new Trade("trade2", "Mehmet", new Card("2", "WaterSpell", 40, "Spell"), "Spell", 20));

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
            else if (method == "GET" && path == "/cards")
            {
                return HandleGetCards(authHeader);
            }
            else if (method == "PUT" && path == "/deck")
            {
                return HandleSetDeck(authHeader, requestBody);
            }
            else if (method == "GET" && path == "/deck")
            {
                return HandleGetDeck(authHeader);
            }
            else if (method == "POST" && path == "/battles")
            {
                return HandleBattle(authHeader);
            }
            else if (method == "GET" && path == "/scoreboard")
            {
                return HandleGetScoreboard();
            }
            else if (method == "GET" && path == "/stats")  // ✅ NEU: `GET /stats`
            {
                return HandleGetStats(authHeader);
            }
            else if (method == "GET" && path == "/tradings")
            {
                return HandleGetTradings();
            }
            else if (method == "POST" && path == "/tradings")
            {
                return HandleCreateTrade(authHeader, requestBody);
            }
            else if (method == "POST" && path.StartsWith("/tradings/"))
            {
                return HandleAcceptTrade(path, authHeader, requestBody);
            }
            else if (method == "DELETE" && path.StartsWith("/tradings/"))
            {
                return HandleDeleteTrade(path, authHeader);
            }
            else if (method == "GET" && path == "/users")
            {
                return HandleGetAllUsersWithAdo();
            }
            else if (method == "POST" && path == "/packages")
            {
                return HandlePostPackage(authHeader, requestBody);
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

        private string HandleGetCards(string authHeader)
        {
            Console.WriteLine($"DEBUG: Token erhalten: {authHeader}");

            if (string.IsNullOrEmpty(authHeader))
            {
                Console.WriteLine("DEBUG: Kein Token erhalten!");
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", ""); // Token in Username umwandeln
            Console.WriteLine($"DEBUG: Extrahierter Username: {username}");

            if (!_userService.ValidateToken(username, authHeader))
            {
                Console.WriteLine("DEBUG: Token ungültig!");
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUngültiger Token";
            }

            List<Card> userCards = _cardService.GetUserCards(username);
            Console.WriteLine($"DEBUG: Anzahl Karten gefunden: {userCards.Count}");

            if (userCards.Count == 0)
            {
                Console.WriteLine("DEBUG: Keine Karten vorhanden!");
                return "HTTP/1.1 204 No Content\r\nContent-Type: text/plain\r\n\r\nUser hat keine Karten";
            }

            string jsonResponse = JsonSerializer.Serialize(userCards);
            Console.WriteLine($"DEBUG: JSON-Antwort: {jsonResponse}");
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }

        private string HandleSetDeck(string authHeader, string requestBody)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", ""); // Token in Username umwandeln

            if (!_userService.ValidateToken(username, authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUngültiger Token";
            }

            try
            {
                List<string>? cardIds = JsonSerializer.Deserialize<List<string>>(requestBody);
                if (cardIds == null || cardIds.Count != 4)
                {
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nDeck muss genau 4 Karten enthalten";
                }

                List<Card> userCards = _cardService.GetUserCards(username);
                List<Card> selectedCards = userCards.Where(c => cardIds.Contains(c.Id)).ToList();

                if (selectedCards.Count != 4)
                {
                    return "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nMindestens eine Karte gehört nicht dem User";
                }

                _deckService.SetUserDeck(username, selectedCards);
                return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nDeck erfolgreich gespeichert";
            }
            catch
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nUngültiges JSON-Format";
            }
        }

        private string HandleGetDeck(string authHeader)
        {
            Console.WriteLine("DEBUG: `GET /deck` aufgerufen!");

            if (string.IsNullOrEmpty(authHeader))
            {
                Console.WriteLine("DEBUG: Kein Token angegeben!");
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", ""); // Token in Username umwandeln
            Console.WriteLine($"DEBUG: Extrahierter Username: {username}");

            if (!_userService.ValidateToken(username, authHeader))
            {
                Console.WriteLine("DEBUG: Ungültiger Token!");
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUngültiger Token";
            }

            Deck? userDeck = _deckService.GetUserDeck(username);
            if (userDeck == null || userDeck.Cards.Count == 0)
            {
                Console.WriteLine("DEBUG: Deck ist leer!");
                return "HTTP/1.1 204 No Content\r\nContent-Type: text/plain\r\n\r\nDeck ist leer";
            }

            string jsonResponse = JsonSerializer.Serialize(userDeck.Cards);
            Console.WriteLine($"DEBUG: JSON-Antwort: {jsonResponse}");
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }
        private string HandleBattle(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", ""); // Token in Username umwandeln

            if (!_userService.ValidateToken(username, authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUngültiger Token";
            }

            string battleResult = _battleService.JoinBattle(username);
            return $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{battleResult}";
        }

        private string HandleGetScoreboard()
        {
            List<User> sortedUsers = _userService.GetUsersSortedByElo();

            string jsonResponse = JsonSerializer.Serialize(sortedUsers.Select(u => new
            {
                u.Username,
                u.Elo,
                u.Wins,
                u.Losses
            }));

            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }

        private string HandleGetStats(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", ""); // Token in Username umwandeln

            if (!_userService.ValidateToken(username, authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUngültiger Token";
            }

            User? user = _userService.GetUser(username);
            if (user == null)
            {
                return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser nicht gefunden";
            }

            string jsonResponse = JsonSerializer.Serialize(new
            {
                user.Username,
                user.Elo,
                user.Wins,
                user.Losses
            });

            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }
        private string HandleGetTradings()
        {
            List<Trade> trades = _tradingService.GetActiveTrades();

            if (trades.Count == 0)
            {
                return "HTTP/1.1 204 No Content\r\nContent-Type: text/plain\r\n\r\nKeine aktiven Trades verfügbar";
            }

            string jsonResponse = JsonSerializer.Serialize(trades);
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }

        private string HandleCreateTrade(string authHeader, string requestBody)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", ""); // Token in Username umwandeln

            if (!_userService.ValidateToken(username, authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUngültiger Token";
            }

            try
            {
                Trade? trade = JsonSerializer.Deserialize<Trade>(requestBody);
                if (trade == null || string.IsNullOrEmpty(trade.Id) || string.IsNullOrEmpty(trade.RequiredType))
                {
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nUngültige Trade-Daten";
                }

                List<Card> userCards = _cardService.GetUserCards(username);
                Card? offeredCard = userCards.FirstOrDefault(c => c.Id == trade.OfferedCard.Id);

                if (offeredCard == null)
                {
                    return "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nKarte gehört nicht dem User";
                }

                _tradingService.AddTrade(new Trade(trade.Id, username, offeredCard, trade.RequiredType, trade.MinimumDamage));

                return "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nTrade erfolgreich erstellt";
            }
            catch
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nFehlerhafte JSON-Daten";
            }
        }
        private string HandleAcceptTrade(string path, string authHeader, string requestBody)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", ""); // Token in Username umwandeln

            if (!_userService.ValidateToken(username, authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUngültiger Token";
            }

            string tradeId = path.Substring(10); // "/tradings/{id}" → Trade-ID extrahieren

            Trade? trade = _tradingService.GetTradeById(tradeId);
            if (trade == null)
            {
                return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nTrade nicht gefunden";
            }

            if (trade.Owner == username)
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nDu kannst deinen eigenen Trade nicht annehmen";
            }

            try
            {
                Card? offeredCard = JsonSerializer.Deserialize<Card>(requestBody);
                if (offeredCard == null)
                {
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nFehlerhafte JSON-Daten";
                }

                List<Card> userCards = _cardService.GetUserCards(username);
                Card? userCard = userCards.FirstOrDefault(c => c.Id == offeredCard.Id);

                if (userCard == null)
                {
                    return "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nKarte gehört nicht dem User";
                }

                if (userCard.Type != trade.RequiredType || userCard.Damage < trade.MinimumDamage)
                {
                    return "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nKarte erfüllt nicht die Anforderungen des Trades";
                }

                // Karten tauschen
                _cardService.RemoveCardFromUser(username, userCard);
                _cardService.AddCardToUser(username, trade.OfferedCard);
                _cardService.RemoveCardFromUser(trade.Owner, trade.OfferedCard);
                _cardService.AddCardToUser(trade.Owner, userCard);

                _tradingService.RemoveTrade(tradeId);

                return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nTrade erfolgreich abgeschlossen";
            }
            catch
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nFehlerhafte JSON-Daten";
            }
        }
        private string HandleDeleteTrade(string path, string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nKein Token angegeben";
            }

            string username = authHeader.Replace("-mtcgToken", "");

            // Trade-ID aus dem Pfad extrahieren: /tradings/{id}
            string tradeId = path.Substring(10);

            Trade? trade = _tradingService.GetTradeById(tradeId);
            if (trade == null)
            {
                return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nTrade nicht gefunden";
            }

            // Überprüfen, ob der aktuelle Benutzer der Besitzer des Trades ist
            if (trade.Owner != username)
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nDu bist nicht der Besitzer dieses Trades";
            }

            // Trade löschen
            _tradingService.RemoveTrade(tradeId);

            return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nTrade erfolgreich gelöscht";
        }
        private string HandleGetAllUsersWithAdo()
        {
            var connectionString = "Host=localhost;Port=5432;Database=mtcgdb;Username=mtcguser;Password=mtcgpassword";
            var userRepo = new UserRepositoryAdo(connectionString);

            var users = userRepo.GetAllUsers();

            if (users.Count() == 0)
            {
                return "HTTP/1.1 204 No Content\r\nContent-Type: text/plain\r\n\r\nKeine User vorhanden";
            }

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(users);
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }
        private string HandlePostPackage(string authHeader, string requestBody)
        {
            try
            {
                var cards = JsonSerializer.Deserialize<List<Card>>(requestBody);

                if (cards == null || cards.Count != 5)
                {
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nEin Paket muss genau 5 Karten enthalten";
                }

                using var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=mtcgdb;Username=mtcguser;Password=mtcgpassword");
                connection.Open();

                using var transaction = connection.BeginTransaction();

                foreach (var card in cards)
                {
                    var command = new NpgsqlCommand(
                        "INSERT INTO cards (id, name, damage, type, owner_id) VALUES (@id, @name, @damage, @type, NULL)", connection);

                    command.Parameters.AddWithValue("@id", Guid.Parse(card.Id));
                    command.Parameters.AddWithValue("@name", card.Name);
                    command.Parameters.AddWithValue("@damage", card.Damage);
                    command.Parameters.AddWithValue("@type", card.Type);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                return "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nKartenpaket erfolgreich erstellt";
            }
            catch (Exception ex)
            {
                return $"HTTP/1.1 500 Internal Server Error\r\nContent-Type: text/plain\r\n\r\nFehler beim Erstellen des Kartenpakets: {ex.Message}";
            }
        }
    }
}