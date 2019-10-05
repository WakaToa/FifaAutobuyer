using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.MuleApi.Responses
{
    public class MuleFactoryGetPlayerResponse
    {
        public int code { get; set; }
        public string error { get; set; }
        public int transactionID { get; set; }
        public long tradeID { get; set; }
        public int assetID { get; set; }
        public string position { get; set; }
        public int startPrice { get; set; }
        public int coinAmount { get; set; }
        public double paymentInUsd { get; set; }
        public int lockExpires { get; set; }
        public int rating { get; set; }
        public int cardValue { get; set; }
        public bool cardAndFeeDeducted { get; set; }
    }
}
