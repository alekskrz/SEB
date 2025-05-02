using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEB.Models;

namespace SEB.Utils
{
    public class TokenManager
    {
        public static List<User> UsersReference;

        public static User GetUserByToken(string token) 
        {
            if (UsersReference == null)
            {
                return null;
            }
            return UsersReference.FirstOrDefault(u => u.Token == token);
        }
    }
}
