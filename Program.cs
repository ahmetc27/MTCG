using MTCG.Server;

class Program
{
    static void Main()
    {
        HttpServer server = new HttpServer(10001);
        server.Start();
    }
}
