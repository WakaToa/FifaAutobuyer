using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.WebServer.Models
{
    public class ProfitLogsModel : BaseModel
    {
        public string Title { get; set; }
        public string FooterPrevious { get; set; }
        public string FooterNext { get; set; }
        public List<SingleDataLog> Logs { get; set; }
        public string LogType { get; set; }

        public int TotalBuyPrice => Logs.Sum(x => x.BuyPrice);
        public int TotalSellPrice => Logs.Sum(x => x.SellPrice);
        public int TotalProfit => Logs.Sum(x => x.Profit);

        public int AverageBuyPrice
        {
            get
            {
                if (Logs.Count == 0)
                {
                    return 0;
                }
                return Logs.Sum(x => x.BuyPrice) / Logs.Count;
            }
        }
        public int AverageSellPrice
        {
            get
            {
                if (Logs.Count == 0)
                {
                    return 0;
                }
                return Logs.Sum(x => x.SellPrice) / Logs.Count;
            }
        }
        public int AverageProfit
        {
            get
            {
                if (Logs.Count == 0)
                {
                    return 0;
                }
                return Logs.Sum(x => x.Profit) / Logs.Count;
            }
        }
        public class SingleDataLog
        {
            public long ID { get; set; }
            public string ItemName { get; set; }
            public int BuyPrice { get; set; }
            public int SellPrice { get; set; }
            public int Profit { get; set; }
            public string BoughtOn { get; set; }
            public string SoldOn { get; set; }
            public string TimeOnTradepile { get; set; }
            public int ResourceID { get; set; }
            public int RevisionID { get; set; }
        }
    }
}
