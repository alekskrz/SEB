using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEB.Models
{
    public class PushupRecordEntries
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Count { get; set; }
        public int Duration { get; set; }
        public DateTime TimeStamp { get; set; }
        public PushupRecordEntries() 
        {
            TimeStamp = DateTime.Now;
        }
    }
}
