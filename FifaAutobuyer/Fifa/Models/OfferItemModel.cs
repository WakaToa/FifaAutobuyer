using FifaAutobuyer.Fifa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class OfferItemModel
    {
        public long ItemID { get; set; }
        public int StartingBid { get; set; }
        public int Duration { get; set; }
        public int BuyNowPrice { get; set; }

        public OfferItemModel(long itemID, int buyNowPrice)
        {
            Duration = 3600;
            BuyNowPrice = buyNowPrice;
            StartingBid = buyNowPrice.CalculateStartingBid();
            ItemID = itemID;
        }

        public string ToPostData()
        {
            return "{\"startingBid\":" + StartingBid + ",\"duration\":" + Duration + ",\"itemData\":{\"id\":" + ItemID + "},\"buyNowPrice\":" + BuyNowPrice + "}";
        }
        
    }
}
