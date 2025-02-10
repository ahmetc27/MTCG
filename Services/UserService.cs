using System.Collections.Generic;
using MTCG.Models;

namespace MTCG.Services
{
    public class UserService
    {
        private readonly List<User> _users = new(); // Später mit DB ersetzen

        public bool RegisterUser(string username, string password)
        {
            if (_users.Exists(u => u.Username == username))
                return false; // User existiert bereits

            _users.Add(new User(username, password));
            return true; // Erfolgreich registriert
        }
    }
}