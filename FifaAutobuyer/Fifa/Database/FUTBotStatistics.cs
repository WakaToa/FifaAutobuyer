using FifaAutobuyer.Fifa.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Extensions;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futbotstatistics")]
    public class FUTBotStatistics
    {
        [Key]
        public int ID { get; set; }
        public long TotalCoins { get; set; }
        public long TotalTradepileItems { get; set; }
        public long TotalTradepileValue { get; set; }
        public long Buys { get; set; }
        public long Sells { get; set; }
        public long Timestamp { get; set; }


        public FUTBotStatistics()
        {
            Timestamp = Helper.CreateTimestamp();
        }

        public string BuildedStatisticString => "{y: '" + Helper.TimestampToDateTime(Timestamp).ToShortTimeString() + "', item1: " + RoundCloseHundred((int)TotalCoins + (int)TotalTradepileValue) +
                                                ", item2: " + RoundCloseHundred((int)TotalCoins) + ", item3: " + RoundCloseHundred((int)TotalTradepileValue) + "},";

        private int RoundCloseHundred(int number)
        {
            return (int)Math.Round(number / 1000D) * 1000;
        }
    }
}
