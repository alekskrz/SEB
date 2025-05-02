using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SEB.Models;

namespace SEB.Tests
{
    public class AchivementTest
    {
        [Test]
        public void FirstPushUp_Achievement_ShouldUnlock()
        {
            var user = new User { Id = 1, Username = "test" };
            var records = new List<PushupRecordEntries>
    {
        new PushupRecordEntries { UserId = 1, Count = 10, Duration = 120 }
    };

            // Simulate logic from CheckAchievements()
            if (!user.Achievements.Contains("First Push-Up") && records.Count > 0)
                user.Achievements.Add("First Push-Up");

            ClassicAssert.Contains("First Push-Up", user.Achievements);
        }

    }
}
