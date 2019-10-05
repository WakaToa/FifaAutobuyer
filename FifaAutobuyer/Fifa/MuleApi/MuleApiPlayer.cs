using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.MuleApi.Clients;

namespace FifaAutobuyer.Fifa.MuleApi
{
    public class MuleApiPlayer
    {
        public MuleApiType MuleApiType { get; set; }
        public ApiClient MuleApiClient { get; set; }
        public long TransactionId { get; set; }
        public double Revenue { get; set; }

        public int AssetId { get; set; }
        public string Name { get; set; }
        public long TradeId { get; set; }
        public long ItemId { get; set; }

        public int StartingBid { get; set; }
        public int BuyNowPrice { get; set; }
        public int MuleValue { get; set; }

        public DateTime LockStart { get; set; }
        public DateTime LockEnd { get; set; }
    }
}
