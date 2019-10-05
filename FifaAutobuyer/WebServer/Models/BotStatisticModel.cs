using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.WebServer.Models
{
    class BotStatisticModel : BaseModel
    {
        public List<FUTBotStatistics> BotStatistic { get; set; }
        public int TotalProfit { get; set; }

        public BotStatisticModel()
        {
            BotStatisticActive = "active";
        }
    }
}
