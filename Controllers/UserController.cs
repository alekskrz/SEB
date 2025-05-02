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
        private static List<PushupRecordEntries> records = new List<PushupRecordEntries>();
        private static int nextRecordId = 1;
        private readonly DatabaseManager db = new DatabaseManager();

        public void Register(Stream stream, string body) 
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
                if (!data.ContainsKey("Username") || !data.ContainsKey("Password"))
                {
                    SendBadRequest(stream, "Missing username or password.");
                    return;
                }
                string username = data["Username"];
                string password = data["Password"];
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
                if (!data.ContainsKey("Username") || !data.ContainsKey("Password"))
                {
                    SendBadRequest(stream, "Missing username or password.");
                    return;
                }

                string username = data["Username"];
                string password = data["Password"];

                var user = db.GetUserByUsername(username);

                if (user == null || user.Password != password)
                {
                    SendBadRequest(stream, "Invalid credentials.");
                    return;
                }
                user.Token = $"{user.Username}-sebToken";

                db.UpdateUserToken(user.Id, user.Token);
                Console.WriteLine($"User logged in: {username}");

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

                string countStr = data.ContainsKey("Count") ? data["Count"] : data.GetValueOrDefault("count");
                string durationStr = data.ContainsKey("DurationInSeconds") ? data["DurationInSeconds"] : data.GetValueOrDefault("duration");

                if (string.IsNullOrEmpty(countStr) || string.IsNullOrEmpty(durationStr))
                {
                    SendBadRequest(stream, "Missing count or duration.");
                    return;
                }

                int count = int.Parse(countStr);
                int duration = int.Parse(durationStr);



                var record = new PushupRecordEntries
                {
                    UserId = user.Id,
                    Count = count,
                    Duration = duration,
                    TimeStamp = DateTime.Now
                };

                //records.Add(record);
                Console.WriteLine($"Inserting record into DB: UserId={record.UserId}, Count={record.Count}, Duration={record.Duration}");
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

                CheckAchievements(user);
                db.UpdateUserEloAndAchievements(user);

                Console.WriteLine($"Push-up saved + tournament processed for {user.Username}");

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
        public void GetUser(Stream stream, string token, string targetUsername)
        {
            var user = db.GetUserByToken(token);
            if (user == null || user.Username != targetUsername)
            {
                SendUnauthorized(stream);
                return;
            }

            string json = JsonConvert.SerializeObject(new
            {
                Name = user.Username,
                Bio = "TODO: Add Bio field",
                Image = "TODO: Add Image field"
            });

            string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + json;
            stream.Write(Encoding.UTF8.GetBytes(response));
        }

        public void EditUser(Stream stream, string token, string targetUsername, string body)
        {
            var user = db.GetUserByToken(token);
            if (user == null || user.Username != targetUsername)
            {
                SendUnauthorized(stream);
                return;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
                string name = data.ContainsKey("Name") ? data["Name"] : user.Username;
                string bio = data.ContainsKey("Bio") ? data["Bio"] : "TODO";
                string image = data.ContainsKey("Image") ? data["Image"] : "TODO";

                // Update DB if needed here — for now, just pretend it's saved

                Console.WriteLine($"User {user.Username} updated profile: Name={name}, Bio={bio}, Image={image}");

                SendOk(stream, "Profile updated successfully.");
            }
            catch (Exception ex)
            {
                SendBadRequest(stream, "Invalid profile update. " + ex.Message);
            }
        }
        public void GetTournament(Stream stream, string token)
        {
            var response = new
            {
                message = "No tournament data yet. This is a stub."
            };

            string json = JsonConvert.SerializeObject(response);
            string reply = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + json;
            stream.Write(Encoding.UTF8.GetBytes(reply));
        }

    }
}
