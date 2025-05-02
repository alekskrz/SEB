using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using SEB.Controllers;
using Newtonsoft.Json.Linq;

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
                string token = null;
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
                /*while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    if (line.StartsWith("Content-Length:"))
                    {
                        contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
                    }
                }*/
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    if (line.StartsWith("Content-Length:"))
                    {
                        contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
                    }
                    else if (line.StartsWith("Authorization: Basic"))
                    {
                        token = line.Substring("Authorization: Basic".Length).Trim();
                    }
                }

                // Step 3: Read the exact number of characters from the body
                char[] buffer = new char[contentLength];
                int read = reader.Read(buffer, 0, contentLength);
                string body = new string(buffer);

                Console.WriteLine("Body: " + body);
                UserController controller = new UserController();

                if (method == "POST" && path == "/users")
                {
                    //HandleRegister(stream, body);
                    controller.Register(stream, body);
                }
                else if(method == "POST" && path == "/sessions") 
                {
                    // HandleLogin(stream, body);
                    controller.Login(stream, body);
                }
                else if (method == "GET" && path == "/history")
                {
                    //HandleUserHistory(stream, token);
                    controller.GetHistory(stream, token);
                }
                else if (method == "POST" && path == "/history")
                {
                    //HandlePushup(stream, body, token);
                    controller.AddPushupRecord(stream, token, body);

                }
                else if (method == "GET" && path == "/stats")
                {
                    //HandleUserStats(stream, token);
                    controller.GetStats(stream, token);
                }
                else if (method == "GET" && path == "/score")
                {
                    //HandleScoreboard(stream);
                    try
                    {
                        controller.GetScoreboard(stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Scoreboard error: " + ex.Message);
                        //SendOk(stream, "Scoreboard temporarily unavailable.");
                    }

                }
                else if (method == "GET" && path == "/user/achievements")
                {
                    //HandleAchievements(stream, token);
                    controller.GetAchievements(stream, token);

                }
                else if (method == "GET" && path.StartsWith("/users/"))
                {
                    string targetUser = path.Substring("/users/".Length);
                    controller.GetUser(stream, token, targetUser);
                }
                else if (method == "PUT" && path.StartsWith("/users/"))
                {
                    string targetUser = path.Substring("/users/".Length);
                    controller.EditUser(stream, token, targetUser, body);
                }
                /*else if (method == "GET" && path == "/tournament")
                {
                    controller.GetTournament(stream, token);
                }*/
                else if (method == "GET" && path == "/tournament")
                {
                    controller.GetTournament(stream, token);
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
            //StreamReader reader = new StreamReader (stream);
            //string body = ReadRequestBody(reader);
            UserController controller = new UserController();
            controller.Register(stream, body);
            Console.WriteLine("HandleRequest 5");
            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);

        }
        private void HandleLogin(NetworkStream stream, string body) 
        {
            //StreamReader reader = new StreamReader (stream);
            //string body = ReadRequestBody(reader);
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

        private void HandleUserHistory(NetworkStream stream, string token) 
        {
            /*StreamReader reader = new StreamReader(stream);

            string line;
            string token = null;
            while(!string.IsNullOrEmpty(line = reader.ReadLine())) 
            {
                if (line.StartsWith("Authorization: Basic")) 
                {
                    token = line.Substring("Authorization: Basic".Length).Trim();
                }
            }*/
            UserController controller = new UserController();
            controller.GetHistory(stream, token);
        }

        private void HandlePushup(NetworkStream stream, string body, string token)
        {
            //StreamReader reader = new StreamReader(stream);
            //string line;
            //string token = null;
            //int contentLength = 0;

            // Read headers
            /*while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Authorization: Basic"))
                {
                    token = line.Substring("Authorization: Basic".Length).Trim();
                }
                else if (line.StartsWith("Content-Length:"))
                {
                    contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
                }
            }*/

            // Read body
            /*char[] buffer = new char[contentLength];
            reader.Read(buffer, 0, contentLength);
            string body = new string(buffer);*/

            UserController controller = new UserController();
            controller.AddPushupRecord(stream, token, body);
        }
        private void HandleUserStats(NetworkStream stream, string token)
        {
            //StreamReader reader = new StreamReader(stream);
            //string line;
            /*string token = null;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Authorization: Basic"))
                {
                    token = line.Substring("Authorization: Basic".Length).Trim();
                }
            }*/

            UserController controller = new UserController();
            controller.GetStats(stream, token);
        }
        private void HandleScoreboard(NetworkStream stream)
        {
            UserController controller = new UserController();
            controller.GetScoreboard(stream);
        }
        private void HandleAchievements(NetworkStream stream, string token)
        {
            /*StreamReader reader = new StreamReader(stream);
            string line;
            string token = null;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Authorization:"))
                {
                    token = line.Substring("Authorization:".Length).Trim().Replace("Bearer ", "");
                }
            }*/

            UserController controller = new UserController();
            controller.GetAchievements(stream, token);
        }


    }
}
/*using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using SEB.Controllers;

namespace SEB.Server
{
    public class Router
    {
        public void HandleRequest(NetworkStream stream)
        {
            try
            {
                /*string line;
                int contentLength = 0;
                string token = null;
                StreamReader reader = new StreamReader(stream);
                string requestLine = reader.ReadLine();
                if (requestLine == null)
                {
                    Console.WriteLine("Empty request received.");
                    return;
                }

                Console.WriteLine($"Received request: {requestLine}");

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
                    else if (line.StartsWith("Authorization: Basic"))
                    {
                        token = line.Substring("Authorization: Basic".Length).Trim();
                    }
                }

                char[] buffer = new char[contentLength];
                int read = reader.Read(buffer, 0, contentLength);
                string body = new string(buffer);

                Console.WriteLine("Body: " + body);

                UserController controller = new UserController();

                if (method == "POST" && path == "/users")
                {
                    controller.Register(stream, body);
                }
                else if (method == "POST" && path == "/sessions")
                {
                    controller.Login(stream, body);
                }
                else if (method == "GET" && path == "/history")
                {
                    controller.GetHistory(stream, token);
                }
                else if (method == "POST" && path == "/history")
                {
                    controller.AddPushupRecord(stream, token, body);
                }
                else if (method == "GET" && path == "/stats")
                {
                    controller.GetStats(stream, token);
                }
                else if (method == "GET" && path == "/score")
                {
                    controller.GetScoreboard(stream);
                }
                else if (method == "GET" && path == "/tournament")
                {
                    SendOk(stream, "Tournament endpoint not yet implemented.");
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

        private void SendOk(NetworkStream stream, string message)
        {
            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + message;
            stream.Write(Encoding.UTF8.GetBytes(response));
        }

        private void SendNotFound(NetworkStream stream)
        {
            string response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nRoute not found.";
            stream.Write(Encoding.UTF8.GetBytes(response));
        }
    }
}*/

