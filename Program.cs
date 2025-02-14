using MTCG.Server;
using MTCG.Repositories;

class Program
{
    static void Main()
    {
        HttpServer server = new HttpServer(10001);
        server.Start();

        var connectionString = "Host=localhost;Port=5432;Database=mtcgdb;Username=mtcguser;Password=mtcgpassword";
        var dbRepo = new DatabaseRepository(connectionString);

        // Testabfrage
        var users = dbRepo.ExecuteQuery("SELECT * FROM users");
        foreach (var user in users)
        {
            Console.WriteLine($"{user.username} - ELO: {user.elo}");
        }
    }
}
