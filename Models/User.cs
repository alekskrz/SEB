using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEB.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Elo { get; set; }
        public string Token { get; set; }
        public List<string> Achievements { get; set; }
        public User() 
        {
            Elo = 1000;
            Achievements = new List<string>();
        }
    }
}
