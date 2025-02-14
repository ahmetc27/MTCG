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
                new Card("1", "FireDragon", 50, "Monster"),
                new Card("2", "WaterGoblin", 30, "Monster"),
                new Card("3", "RegularOrk", 40, "Monster"),
                new Card("4", "WaterSpell", 35, "Spell"),
                new Card("5", "LightningDragon", 55, "Monster"),
                new Card("6", "EarthGolem", 32, "Monster"),
                new Card("7", "ShadowElf", 45, "Monster"),
                new Card("8", "FireSpell", 38, "Spell")
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
        public void UpdateElo(string username, int eloChange)
        {
            User? user = _users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                user.Elo += eloChange;
                if (eloChange > 0) user.Wins++;
                else if (eloChange < 0) user.Losses++; 
            }
        }
        public List<User> GetUsersSortedByElo()
        {
            return _users.OrderByDescending(u => u.Elo).ToList();
        }
    }
}
