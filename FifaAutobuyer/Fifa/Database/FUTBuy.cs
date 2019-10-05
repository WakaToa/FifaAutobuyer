﻿using System;
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
    [Table("futbuys")]
    public class FUTBuy
    {
        [Key]
        public long ID { get; set; }
        public FUTBuyBidType Type { get; set; }
        public string EMail { get; set; }
        public long TradeID { get; set; }
        public int BuyNowPrice { get; set; }
        public int AssetID { get; set; }
        public int RevisionID { get; set; }
        public long Timestamp { get; set; }

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

        public FUTBuy()
        {
            ID = -1;
            TradeID = -1;
            BuyNowPrice = -1;
            AssetID = -1;
            RevisionID = -1;
            Timestamp = -1;
        }
    }
}
