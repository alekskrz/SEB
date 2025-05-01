using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SEB.Server
{
    public class HttpServer
    {
        private int _port;
        private TcpListener _listener;

        public HttpServer(int port) 
        {
            _port = port;
        }

        public void Start() 
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"Server is listening on port {_port}...");

            while (true) 
            {
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("Client connected");

                //Handle the client in a new Method (this will be done later)
                HandleClient(client);
            }
        }

        public void HandleClient(TcpClient client) 
        {
            using (NetworkStream stream = client.GetStream()) 
            {
                Router router = new Router();
                router.HandleRequest(stream);
            }
            Console.WriteLine("Handling client...");
            client.Close();
        }
    }
}
