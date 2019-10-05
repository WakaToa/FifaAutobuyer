using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class AuctionInfo
    {
        public long tradeId { get; set; }
        public ItemData itemData { get; set; }
        public string tradeState { get; set; }
        public int buyNowPrice { get; set; }
        public int currentBid { get; set; }
        public int offers { get; set; }
        public object watched { get; set; }
        public string bidState { get; set; }
        public int startingBid { get; set; }
        public int confidenceValue { get; set; }
        public int expires { get; set; }
        public string sellerName { get; set; }
        public int sellerEstablished { get; set; }
        public int sellerId { get; set; }
        public object tradeOwner { get; set; }
        public int coinsProcessed { get; set; }
    }
}
