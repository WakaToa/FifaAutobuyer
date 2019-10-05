using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.MuleApi.Responses
{
    public class GameTradeEasyResponse
    {
        public string error { get; set; }
        public string message { get; set; }
        public double price { get; set; }
        public string currency { get; set; }
        public List<GameTradeEasyPlayerResponse> player { get; set; }


        public class GameTradeEasyPlayerResponse
        {
            public string tradeId { get; set; }
            public string tradeState { get; set; }
            public string bidState { get; set; }
            public int startingBid { get; set; }
            public int currentBid { get; set; }
            public int buyNowPrice { get; set; }
            public int expires { get; set; }
            public int assetId { get; set; }
            public string name { get; set; }
            public long itemId { get; set; }
            public int resourceId { get; set; }
            public int rating { get; set; }
            public int rareflag { get; set; }
            public string preferredPosition { get; set; }
            public int fitness { get; set; }
            public int contract { get; set; }
        }
    }

    public class GameTradeEasyResponse2
    {
        public string error { get; set; }
        public string message { get; set; }
        public double price { get; set; }
        public string currency { get; set; }
        public GameTradeEasyResponse.GameTradeEasyPlayerResponse player { get; set; }
    }
}
