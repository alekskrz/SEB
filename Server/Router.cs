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

                if (method == "POST" && path == "/register")
                {
                    HandleRegister(stream);
                }
                else if(method == "POST" && path == "/login") 
                {
                    HandleLogin(stream);
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
        private void HandleRegister(NetworkStream stream) 
        {
            /*string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser registration not implemented yet.";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);*/
            StreamReader reader = new StreamReader (stream);
            string body = ReadRequestBody(reader);
            UserController controller = new UserController();
            controller.Register(stream, body);

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
    }
}
