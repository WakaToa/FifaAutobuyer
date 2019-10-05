using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futitemprofits")]
    public class FUTItemProfit
    {
        public long ID { get; set; }
        public FUTBuyBidType Type { get; set; }
        public long ItemID { get; set; }
        public int AssetID { get; set; }
        public int RevisionID { get; set; }
        public string Position { get; set; }
        public ChemistryStyle ChemistryStyle { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int Profit { get; set; }
        public long BuyTimestamp { get; set; }
        public long SellTimestamp { get; set; }
        public string Account { get; set; }

        public string ItemName
        {
            get
            {
                var item = FUTItemManager.GetItemByAssetRevisionID(AssetID, RevisionID);
                return item != null ? item.GetName() : "";
            }
        }

        public int Rating
        {
            get
            {
                var itemData = FUTItemManager.GetItemByAssetRevisionID(AssetID, RevisionID);
                return itemData?.r ?? 0;
            }
        }

        public FUTItemProfit()
        {
            ItemID = 0;
            BuyPrice = 0;
            SellPrice = 0;
            Profit = 0;
        }
    }
}
