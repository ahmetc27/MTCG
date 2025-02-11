using System.Collections.Generic;
using MTCG.Models;

namespace MTCG.Services
{
    public class UserService
    {
        private readonly List<User> _users = new(); // Später mit DB ersetzen
        private readonly CardService _cardService; // Neue Instanz für Karten-Service

        public UserService(CardService cardService)
        {
            _cardService = cardService;
        }
        public bool RegisterUser(string username, string password)
        {
            if (_users.Exists(u => u.Username == username))
                return false;

            _users.Add(new User(username, password));

            // Testweise Karten hinzufügen
            Console.WriteLine($"DEBUG: Karten für {username} werden hinzugefügt...");
            _cardService.AddCardsToUser(username, new List<Card>
            {
                new Card("1", "FireDragon", 50),
                new Card("2", "WaterGoblin", 30),
                new Card("3", "RegularOrk", 40),
                new Card("4", "WaterSpell", 35)
            });
            Console.WriteLine("DEBUG: Karten erfolgreich hinzugefügt!");

            return true;
        }



        public string? AuthenticateUser(string username, string password)
        {
            User? user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            
            if (user == null)
                return null; // ❌ Falsche Login-Daten

            return $"{username}-mtcgToken"; // ✅ Token wird generiert
        }

        public bool ValidateToken(string username, string token)
        {
            return token == $"{username}-mtcgToken";
        }

        public User? GetUser(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }
    }
}
