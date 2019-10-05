using FifaAutobuyer.Fifa.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futbotlogs")]
    public class FUTBotLog
    {
        [Key]
        public int ID { get; set; }
        public string EMail { get; set; }
        public string Data { get; set; }
        public long Timestamp { get; set; }

        public FUTBotLog(string email, string data)
        {
            EMail = email;
            Data = data;
            Timestamp = Helper.CreateTimestamp();
        }

        public FUTBotLog()
        {

        }
    }
}
