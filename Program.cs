using SEB.Server;

namespace SEB
{
    class Program 
    {
        static void Main(string[] args) 
        {
            Console.WriteLine("Staring Sports Exercise Battle Server...");

            //Create and start the server
            HttpServer server = new HttpServer(10001);
            server.Start();
        }
    }
}
