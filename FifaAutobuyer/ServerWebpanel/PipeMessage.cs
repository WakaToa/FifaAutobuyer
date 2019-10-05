using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.ServerWebpanel
{
    [Serializable]
    public class ClientOverview
    {
        public string BotInstance { get; set; }
        public string Platform { get; set; }
        public int Accounts { get; set; }
        public int AccountsRunning { get; set; }
        public int AverageCoinsPerAccount { get; set; }
        public int ProfitLast24Hours { get; set; }
        public int TotalCoins { get; set; }
        public int TotalTradepileItems { get; set; }
        public int TotalTradepileValue { get; set; }
        public int TotalOverallValue { get; set; }
        public int Notifications { get; set; }
        public string BotStatistic { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
