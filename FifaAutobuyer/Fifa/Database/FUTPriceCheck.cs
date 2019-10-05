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
    [Table("futpricechecks")]
    public class FUTPriceCheck
    {
        [Key]
        public int ID { get; set; }

        public int AssetID { get; set; }
        public int RevisionID { get; set; }
        public int AveragePrice { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int BuyPercent { get; set; }
        public int SellPercent { get; set; }
        public long Timestamp { get; set; }

        public FUTPriceCheck(int assetID, int revID, int avgPrice, int buy, int sell, int buyPercent, int sellPercent)
        {
            ID = -1;
            AssetID = assetID;
            RevisionID = revID;
            AveragePrice = avgPrice;
            BuyPrice = buy;
            SellPrice = sell;
            BuyPercent = buyPercent;
            SellPercent = sellPercent;
            Timestamp = Helper.CreateTimestamp();
        }

        public FUTPriceCheck()
        {

        }
    }
}
