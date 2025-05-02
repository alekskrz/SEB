namespace Tests;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SEB.Models;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

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
}
