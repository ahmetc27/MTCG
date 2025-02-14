namespace MTCG.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Elo { get; set; } = 100;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;

        public User(string username, string password)
        {
            Username = username;
            Password = password; // TODO: Hashen für mehr Sicherheit
        }
        public User()
        {
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}