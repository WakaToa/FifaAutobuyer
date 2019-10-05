using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futlogs")]
    public class FUTLog
    {
        [Key]
        public long ID { get; set; }
        public string EMail { get; set; }
        public long TradeID { get; set; }
        public int ResourceID { get; set; }
        public string SellerName { get; set; }
        public int CurrentBid { get; set; }
        public int BuyNowPrice { get; set; }
        public string TradeState { get; set; }
        public int Expires { get; set; }
        public long Timestamp { get; set; }

        public FUTLog()
        {
            ID = -1;
            TradeID = -1;
            ResourceID = -1;
            SellerName = "";
            CurrentBid = -1;
            BuyNowPrice = -1;
            TradeState = "";
            Expires = -1;
            Timestamp = -1;
        }
    }
}
