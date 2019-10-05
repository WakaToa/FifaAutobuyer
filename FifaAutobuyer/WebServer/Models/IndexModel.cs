using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.WebServer.Models
{
    class IndexModel  : BaseModel
    {
        public int TotalCoins { get; set; }
        public int TotalAccounts { get; set; }
        public int AvgCoinsPerAccount { get; set; }
        public int TotalTradepileItems { get; set; }
        public int TotalTradepileValue { get; set; }
        public int TotalOverallValue { get; set; }

        public int TotalLogs { get; set; }
        public int TotalBuys { get; set; }
        public int TotalSells { get; set; }

        public bool DisplayError { get; set; }
        public string DisplayErrorStyle => DisplayError ? "" : "display:none;";
        public string ErrorMessage { get; set; }

        public IndexModel()
        {
            HomeActive = "active";
        }
    }
}
