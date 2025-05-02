using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SEB.Models;
using SEB.Database;

namespace SEB.Controllers
{
    public class UserController
    {
        //private static List<User> _users = new List<User>();
        //private static int nextUserId = 1;
        private static List<PushupRecordEntries> records = new List<PushupRecordEntries>();
        private static int nextRecordId = 1;
        private readonly DatabaseManager db = new DatabaseManager();

        /*static UserController()
        {
            SEB.Utils.TokenManager.UsersReference = _users;
        }*/

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
                if (db.GetUserByUsername(username) != null)
                {
                    SendBadRequest(stream, "Username already taken.");
                }
                //Create a new user
                var user = new User
                {
                    Username = username,
                    Password = password,
                    Elo = 1000,
                    Token = "",
                    Achievements = new List<string>()
                };

                db.InsertUser(user);
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

                var user = db.GetUserByUsername(username);

                if (user == null || user.Password != password)
                {
                    SendBadRequest(stream, "Invalid credentials.");
                    return;
                }
                user.Token = Guid.NewGuid().ToString();
                //user.Token = token;
                db.UpdateUserToken(user.Id, user.Token);
                Console.WriteLine($"User logged in: {username}");

               /* var responseObj = new Dictionary<string, string>
                {
                    { 
                        "token", token
                    }
                };*/
                string jsonResponse = JsonConvert.SerializeObject(new { token = user.Token });

                string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + jsonResponse;
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                SendBadRequest(stream, "Invalid request format. " + ex.Message);
            }
        }

        public void GetHistory(Stream stream, string token) 
        {
            //User user = SEB.Utils.TokenManager.GetUserByToken(token);
            var user  = db.GetUserByToken(token);
            if (user == null) 
            {
                SendUnauthorized(stream);
                return;
            }

            /*var History = new List<object>
            {
                new
                {
                    count = 0, duration = 60, timestamp = "2025-03-30T10:00:00Z"
                }
            };*/
            /*var userRecords = db.GetRecordsByUserId(user.Id)
                .ConvertAll(r => new
                {
                    count = r.Count,
                    duration = r.Duration,
                    timestamp = r.TimeStamp.ToString("o")
                });*/
            var records = db.GetRecordsByUserId(user.Id)
                .Select(r => new
                {
                    count = r.Count,
                    duration = r.Duration,
                    timestamp = r.TimeStamp.ToString("o")
                });

            string json = JsonConvert.SerializeObject(records);

            string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + json;
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }
        private void SendUnauthorized(Stream stream) 
        {
            string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid or missing token.";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }

        public void AddPushupRecord(Stream stream, string token, string body)
        {
            var user = db.GetUserByToken(token);

            if (user == null)
            {
                SendUnauthorized(stream);
                return;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);

                if (!data.ContainsKey("count") || !data.ContainsKey("duration"))
                {
                    SendBadRequest(stream, "Missing count or duration.");
                    return;
                }

                int count = int.Parse(data["count"]);
                int duration = int.Parse(data["duration"]);

                var record = new PushupRecordEntries
                {
                    UserId = user.Id,
                    Count = count,
                    Duration = duration,
                    TimeStamp = DateTime.Now
                };

                //records.Add(record);
                db.InsertPushupRecord(record);
                var recentRecords = db.GetRecordsNear(DateTime.Now, 5);
                var grouped = new Dictionary<int, int>();
                Console.WriteLine($"User {user.Username} added push-up record: {count} reps in {duration}s");
                foreach (var r in recentRecords)
                {
                    if (!grouped.ContainsKey(r.UserId))
                        grouped[r.UserId] = 0;

                    grouped[r.UserId] += r.Count;
                }

                int max = grouped.Values.Max();
                var winners = grouped.Where(p => p.Value == max).Select(p => p.Key).ToList();

                foreach (var entry in grouped)
                {
                    var u = db.GetUserById(entry.Key);
                    if (winners.Contains(entry.Key))
                        u.Elo += (winners.Count > 1 ? 1 : 2);
                    else
                        u.Elo = Math.Max(0, u.Elo - 1);

                    db.UpdateUserEloAndAchievements(u);
                }

                /*List<int> winners = new();
                foreach (var entry in grouped)
                {
                    if (entry.Value == max)
                        winners.Add(entry.Key);
                }

                // Update ELO
                foreach (var entry in grouped)
                {
                    User u = db.GetUserByUsername(usersUsernameFromId(entry.Key)); // you'll need a lookup helper
                    if (winners.Contains(entry.Key))
                        u.Elo += (winners.Count > 1 ? 1 : 2);
                    else
                        u.Elo = Math.Max(0, u.Elo - 1);

                    db.UpdateUserEloAndAchievements(u);
                }*/

                CheckAchievements(user);
                db.UpdateUserEloAndAchievements(user);

                Console.WriteLine($"Push-up saved + tournament processed for {user.Username}");
                // Run tournament: find other players within 5 seconds
                /*List<PushupRecordEntries> nearbyRecords = records.FindAll(r =>
                    r.Id != record.Id &&
                    Math.Abs((r.TimeStamp - DateTime.Now).TotalSeconds) <= 5);

                // Add this user to the list
                nearbyRecords.Add(record);

                // Group by user and sum their counts
                Dictionary<int, int> userTotals = new Dictionary<int, int>();
                foreach (var rec in nearbyRecords)
                {
                    if (!userTotals.ContainsKey(rec.UserId))
                        userTotals[rec.UserId] = 0;

                    userTotals[rec.UserId] += rec.Count;
                }

                // Find max count
                int maxPushups = userTotals.Values.Max();
                List<int> winners = new List<int>();

                foreach (var entry in userTotals)
                {
                    if (entry.Value == maxPushups)
                    {
                        winners.Add(entry.Key);
                    }
                }

                // ELO update
                foreach (var entry in userTotals)
                {
                    var u = _users.Find(x => x.Id == entry.Key);
                    if (u == null) continue;

                    if (winners.Contains(entry.Key))
                        u.Elo += (winners.Count > 1 ? 1 : 2); // Draw: +1, win: +2
                    else
                        u.Elo -= 1;

                    // Clamp to minimum ELO 0
                    if (u.Elo < 0) u.Elo = 0;
                }

                Console.WriteLine("Tournament finished. ELOs updated.");
                CheckAchievements(user);*/

                SendOk(stream, "Push-up record saved!");
            }
            catch (Exception ex)
            {
                SendBadRequest(stream, "Invalid request format. " + ex.Message);
            }
        }
        public void GetStats(Stream stream, string token)
        {
            //User user = SEB.Utils.TokenManager.GetUserByToken(token);
            var user = db.GetUserByToken(token);

            if (user == null)
            {
                SendUnauthorized(stream);
                return;
            }

            // Sum all push-ups by this user
            /*int totalPushups = records
                .FindAll(r => r.UserId == user.Id)
                .Sum(r => r.Count);

            var responseObj = new
            {
                username = user.Username,
                elo = user.Elo,
                totalPushups = totalPushups
            };*/
            var total = db.GetRecordsByUserId(user.Id).Sum(r => r.Count);
            var stats = new
            {
                username = user.Username,
                elo = user.Elo,
                totalPushups = total
            };

            string jsonResponse = JsonConvert.SerializeObject(stats);
            string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + jsonResponse;

            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }
        public void GetScoreboard(Stream stream)
        {
            var users = db.GetAllUsers();
            var scoreboard = users.Select(u =>
            {
                var total = db.GetRecordsByUserId(u.Id).Sum(r => r.Count);
                return new
                {
                    username = u.Username,
                    elo = u.Elo,
                    totalPushups = total
                };
            })
            .OrderByDescending(s => s.elo);
            /*var scoreboard = new List<object>();

            foreach (var user in _users)
            {
                int totalPushups = records
                    .FindAll(r => r.UserId == user.Id)
                    .Sum(r => r.Count);

                scoreboard.Add(new
                {
                    username = user.Username,
                    elo = user.Elo,
                    totalPushups = totalPushups
                });
            }

            // Sort by ELO descending
            scoreboard = scoreboard.OrderByDescending(u => ((dynamic)u).elo).ToList();*/

            string jsonResponse = JsonConvert.SerializeObject(scoreboard);
            string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + jsonResponse;

            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }
        public void GetAchievements(Stream stream, string token)
        {
            //User user = SEB.Utils.TokenManager.GetUserByToken(token);
            var user = db.GetUserByToken(token);

            if (user == null)
            {
                SendUnauthorized(stream);
                return;
            }

            string json = JsonConvert.SerializeObject(user.Achievements);
            string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + json;
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }

        private void CheckAchievements(User user)
        {
            // Avoid duplicates
            //var unlocked = new HashSet<string>(user.Achievements);
            var unlocked = new HashSet<string>(user.Achievements);
            var records = db.GetRecordsByUserId(user.Id);
            int total = records.Sum(r => r.Count);

            // First Push-Up
            if (!unlocked.Contains("First Push-Up") &&
                records.Any(r => r.UserId == user.Id))
            {
                user.Achievements.Add("First Push-Up");
                Console.WriteLine($"{user.Username} unlocked: First Push-Up!");
            }

            // Beast Mode
            if (!unlocked.Contains("Beast Mode") &&
                records.Any(r => r.UserId == user.Id && r.Count >= 100))
            {
                user.Achievements.Add("Beast Mode");
                Console.WriteLine($"{user.Username} unlocked: Beast Mode!");
            }

            // Grinder
            /*int total = records
                .Where(r => r.UserId == user.Id)
                .Sum(r => r.Count);*/

            if (!unlocked.Contains("Grinder") && total >= 500)
            {
                user.Achievements.Add("Grinder");
                Console.WriteLine($"{user.Username} unlocked: Grinder!");
            }

            // Champion (win = +2 ELO, so track wins via ELO gain)
            // We'll assume user started at 1000
            int wins = (user.Elo - 1000) / 2;

            if (!unlocked.Contains("Champion") && wins >= 5)
            {
                user.Achievements.Add("Champion");
                Console.WriteLine($"{user.Username} unlocked: Champion!");
            }
        }



    }
}
