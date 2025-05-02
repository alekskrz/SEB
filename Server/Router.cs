using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using SEB.Controllers;

namespace SEB.Server
{
    public class Router
    {
        public void HandleRequest(NetworkStream stream) 
        {
            try
            {
                string line;
                int contentLength = 0;
                StreamReader reader = new StreamReader(stream);
                string requestLine = reader.ReadLine();
                if (requestLine == null)
                {
                    Console.WriteLine("Empty request recived.");
                    return;
                }

                Console.WriteLine($"Recived request: {requestLine}");

                //Split the request into method and path
                string[] tokens = requestLine.Split(' ');
                if (tokens.Length < 2)
                {
                    Console.WriteLine("Invalid HTTP request line.");
                    return;
                }
                string method = tokens[0];
                string path = tokens[1];
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    if (line.StartsWith("Content-Length:"))
                    {
                        contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
                    }
                }

                // Step 3: Read the exact number of characters from the body
                char[] buffer = new char[contentLength];
                int read = reader.Read(buffer, 0, contentLength);
                string body = new string(buffer);

                Console.WriteLine("Body: " + body);
                if (method == "POST" && path == "/register")
                {
                    HandleRegister(stream, body);
                }
                else if(method == "POST" && path == "/login") 
                {
                    HandleLogin(stream);
                }
                else if (method == "GET" && path == "/user/history")
                {
                    HandleUserHistory(stream);
                }
                else if (method == "POST" && path == "/user/pushup")
                {
                    HandlePushup(stream);
                }
                else if (method == "GET" && path == "/user/stats")
                {
                    HandleUserStats(stream);
                }
                else if (method == "GET" && path == "/scoreboard")
                {
                    HandleScoreboard(stream);
                }
                else if (method == "GET" && path == "/user/achievements")
                {
                    HandleAchievements(stream);
                }
                else
                {
                    SendNotFound(stream);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error handling request: " + ex.Message);
            }
        }
        private void HandleRegister(NetworkStream stream, string body) 
        {
            Console.WriteLine("HandleRequest 1");
            //StreamReader reader = new StreamReader (stream);
            Console.WriteLine("HandleRequest 2");
            //string body = ReadRequestBody(reader);
            Console.WriteLine("HandleRequest 3");
            UserController controller = new UserController();
            Console.WriteLine("HandleRequest 4");
            controller.Register(stream, body);
            Console.WriteLine("HandleRequest 5");
            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser registration not implemented yet.";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);

        }
        private void HandleLogin(NetworkStream stream) 
        {
            StreamReader reader = new StreamReader (stream);
            string body = ReadRequestBody(reader);
            UserController controller = new UserController();
            controller.Login(stream, body);
        }
        private void SendNotFound(NetworkStream stream) 
        {
            string response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nRoute not found.";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }

        private string ReadRequestBody(StreamReader reader) 
        {
            string line;
            int contentlength = 0;

            while (!string.IsNullOrEmpty(line = reader.ReadLine())) 
            {
                if (line.StartsWith("Content-Length:")) 
                {
                    contentlength = int.Parse(line.Substring("Content-Length: ".Length).Trim());
                }
            }
            char[] buffer = new char[contentlength];
            reader.Read(buffer, 0, contentlength);
            return new string(buffer);
        }

        private void HandleUserHistory(NetworkStream stream) 
        {
            StreamReader reader = new StreamReader(stream);

            string line;
            string token = null;
            while(!string.IsNullOrEmpty(line = reader.ReadLine())) 
            {
                if (line.StartsWith("Authorization:")) 
                {
                    token = line.Substring("Authorization:".Length).Trim().Replace("Bearer", "");
                }
            }
            UserController controller = new UserController();
            controller.GetHistory(stream, token);
        }

        private void HandlePushup(NetworkStream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string line;
            string token = null;
            int contentLength = 0;

            // Read headers
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Authorization:"))
                {
                    token = line.Substring("Authorization:".Length).Trim().Replace("Bearer ", "");
                }
                else if (line.StartsWith("Content-Length:"))
                {
                    contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
                }
            }

            // Read body
            char[] buffer = new char[contentLength];
            reader.Read(buffer, 0, contentLength);
            string body = new string(buffer);

            UserController controller = new UserController();
            controller.AddPushupRecord(stream, token, body);
        }
        private void HandleUserStats(NetworkStream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string line;
            string token = null;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Authorization:"))
                {
                    token = line.Substring("Authorization:".Length).Trim().Replace("Bearer ", "");
                }
            }

            UserController controller = new UserController();
            controller.GetStats(stream, token);
        }
        private void HandleScoreboard(NetworkStream stream)
        {
            UserController controller = new UserController();
            controller.GetScoreboard(stream);
        }
        private void HandleAchievements(NetworkStream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string line;
            string token = null;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Authorization:"))
                {
                    token = line.Substring("Authorization:".Length).Trim().Replace("Bearer ", "");
                }
            }

            UserController controller = new UserController();
            controller.GetAchievements(stream, token);
        }


    }
}
