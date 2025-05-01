using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SEB.Models;

namespace SEB.Controllers
{
    public class UserController
    {
        private static List<User> _users = new List<User>();
        private static int nextUserId = 1;

        public void Register(Stream stream, string body) 
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
                if (!data.ContainsKey("username") || !data.ContainsKey("password"))
                {
                    SendBadRequest(stream, "Missing username or password.");
                    return;
                }
                string username = data["username"];
                string password = data["password"];
                if (_users.Exists(u => u.Username == username))
                {
                    SendBadRequest(stream, "Username already taken.");
                }
                //Create a new user
                User user = new User
                {
                    Id = nextUserId++,
                    Username = username,
                    Password = password //Not secure
                };

                _users.Add(user);
                Console.WriteLine($"New user registered: {username}");
                SendOk(stream, "User registered successfully");
            }
            catch (Exception ex) 
            {
                SendBadRequest(stream, "Invalid request format. " +  ex.Message);
            }
        }

        private void SendOk(Stream stream, string message) 
        {
            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + message;
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }

        private void SendBadRequest(Stream stream, string message) 
        {
            string response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\n" + message;
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Login(Stream stream, string body) 
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
                if (!data.ContainsKey("username") || !data.ContainsKey("password"))
                {
                    SendBadRequest(stream, "Missing username or password.");
                    return;
                }

                string username = data["username"];
                string password = data["password"];

                User user = _users.Find(u => u.Username == username && u.Password == password);

                if (user == null)
                {
                    SendBadRequest(stream, "Invalid credentials.");
                    return;
                }
                user.Token = Guid.NewGuid().ToString();
                Console.WriteLine($"User logged in: {username}");
                var responseObj = new Dictionary<string, string>
                {
                    { 
                        "token", user.Token 
                    }
                };
                string jsonResponse = JsonConvert.SerializeObject(responseObj);

                string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + jsonResponse;
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                SendBadRequest(stream, "Invalid request format. " + ex.Message);
            }
        }
    }
}
