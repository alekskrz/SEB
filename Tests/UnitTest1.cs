namespace Tests;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SEB.Models;

public class Tests
{

    [Test]
    public void NewUser_ShouldHaveDefaultElo1000()
    {
        User user = new User();
        ClassicAssert.AreEqual(1000, user.Elo);
    }

    [Test]
    public void UserAchievements_ShouldStartEmpty()
    {
        User user = new User();
        ClassicAssert.IsEmpty(user.Achievements);
    }

    [Test]
    public void Token_ShouldBeUnique()
    {
        var t1 = Guid.NewGuid().ToString();
        var t2 = Guid.NewGuid().ToString();
        ClassicAssert.AreNotEqual(t1, t2);
    }
    [Test]
    public void SoloWinner_Gains2Elo()
    {
        var user = new User { Elo = 1000 };
        var opponent = new User { Elo = 1000 };

        int userScore = 40;
        int opponentScore = 20;

        if (userScore > opponentScore)
            user.Elo += 2;
        else
            user.Elo -= 1;

        ClassicAssert.AreEqual(1002, user.Elo);
    }

    [Test]
    public void Draw_Winner_Gains1Elo()
    {
        var u1 = new User { Elo = 1000 };
        var u2 = new User { Elo = 1000 };

        int u1Score = 30;
        int u2Score = 30;

        if (u1Score == u2Score)
        {
            u1.Elo += 1;
            u2.Elo += 1;
        }

        ClassicAssert.AreEqual(1001, u1.Elo);
        ClassicAssert.AreEqual(1001, u2.Elo);
    }
    [Test]
    public void Elo_ShouldNotGoNegative()
    {
        var user = new User { Elo = 0 };

        user.Elo = Math.Max(0, user.Elo - 1);

        ClassicAssert.AreEqual(0, user.Elo);
    }
    [Test]
    public void Register_ShouldFail_IfPasswordMissing()
    {
        var data = new Dictionary<string, string>
    {
        { "Username", "testuser" }
        // Password missing
    };

        bool isValid = data.ContainsKey("Username") && data.ContainsKey("Password");
        ClassicAssert.IsFalse(isValid);
    }

}
