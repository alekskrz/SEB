using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using SEB.Models;

namespace SEB.Database
{
    public class DatabaseManager
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=Partizan01;Database=EBS";

        public void InsertUser(User user)
        {
            Console.WriteLine("Inserting user 1");
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            Console.WriteLine("Inserting user 2");
            string sql = "INSERT INTO users (username, password, elo, token, achievements) VALUES (@username, @password, @elo, @token, @achievements)";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("password", user.Password);
            cmd.Parameters.AddWithValue("elo", user.Elo);
            cmd.Parameters.AddWithValue("token", user.Token ?? "");
            cmd.Parameters.AddWithValue("achievements", user.Achievements.ToArray());
            cmd.ExecuteNonQuery();
            Console.WriteLine("Inserting user 3");
        }

        public User GetUserByUsername(string username)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "SELECT * FROM users WHERE username = @username";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", username);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Elo = reader.GetInt32(3),
                    Token = reader.GetString(4),
                    Achievements = new List<string>((string[])reader.GetValue(5))
                };
            }

            return null;
        }
        public User GetUserById(int id)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "SELECT * FROM users WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Elo = reader.GetInt32(3),
                    Token = reader.GetString(4),
                    Achievements = new List<string>((string[])reader.GetValue(5))
                };
            }

            return null;
        }

        public User GetUserByToken(string token)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "SELECT * FROM users WHERE token = @token";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("token", token);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Elo = reader.GetInt32(3),
                    Token = reader.GetString(4),
                    Achievements = new List<string>((string[])reader.GetValue(5))
                };
            }

            return null;
        }

        public void UpdateUserToken(int userId, string token)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "UPDATE users SET token = @token WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("token", token);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.ExecuteNonQuery();
        }

        public void UpdateUserEloAndAchievements(User user)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "UPDATE users SET elo = @elo, achievements = @achievements WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("elo", user.Elo);
            cmd.Parameters.AddWithValue("achievements", user.Achievements.ToArray());
            cmd.Parameters.AddWithValue("id", user.Id);
            cmd.ExecuteNonQuery();
        }

        public void InsertPushupRecord(PushupRecordEntries record)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "INSERT INTO pushup_records (user_id, count, duration, timestamp) " +
                         "VALUES (@user_id, @count, @duration, @timestamp)";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", record.UserId);
            cmd.Parameters.AddWithValue("count", record.Count);
            cmd.Parameters.AddWithValue("duration", record.Duration);
            cmd.Parameters.AddWithValue("timestamp", record.TimeStamp);
            cmd.ExecuteNonQuery();
        }

        public List<PushupRecordEntries> GetRecordsByUserId(int userId)
        {
            var result = new List<PushupRecordEntries>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "SELECT id, user_id, count, duration, timestamp FROM pushup_records WHERE user_id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PushupRecordEntries
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Count = reader.GetInt32(2),
                    Duration = reader.GetInt32(3),
                    TimeStamp = reader.GetDateTime(4)
                });
            }

            return result;
        }

        public List<PushupRecordEntries> GetRecordsNear(DateTime timestamp, int seconds)
        {
            var result = new List<PushupRecordEntries>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = @"SELECT id, user_id, count, duration, timestamp
                   FROM pushup_records
                   WHERE ABS(EXTRACT(EPOCH FROM timestamp - @ts)) <= @range";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ts", timestamp);
            cmd.Parameters.AddWithValue("range", seconds);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PushupRecordEntries
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Count = reader.GetInt32(2),
                    Duration = reader.GetInt32(3),
                    TimeStamp = reader.GetDateTime(4)
                });
            }

            return result;
        }
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = "SELECT * FROM users";
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Elo = reader.GetInt32(3),
                    Token = reader.GetString(4),
                    Achievements = new List<string>((string[])reader.GetValue(5))
                });
            }

            return users;
        }

    }
}
