using SEB.Server;

namespace SEB
{
    class Program 
    {
        static void Main(string[] args) 
        {
            Console.WriteLine("Staring Sports Exercise Battle Server...");

            //Create and start the server
            HttpServer server = new HttpServer(8080);
            server.Start();
        }
    }
}
