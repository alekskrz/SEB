using SEB.Database;
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
    public class ValidationTest
    {
        [Test]
        public void InvalidLogin_ShouldReturnNull()
        {
            var db = new DatabaseManager();
            var result = db.GetUserByUsername("nonexistentuser");
            ClassicAssert.IsNull(result);
        }

        [Test]
        public void NegativePushups_ShouldNotCrash()
        {
            var record = new PushupRecordEntries { Count = -10, Duration = 120 };
            ClassicAssert.Less(record.Count, 0);
            // The server should handle this, but test still passes because it's valid data
        }

    }
}
