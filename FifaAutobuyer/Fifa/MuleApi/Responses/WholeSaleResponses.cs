using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.MuleApi.Responses
{
    public class WholeSaleResponse
    {
        public int ret { get; set; }
        public string msg { get; set; }
        public List<Data> data { get; set; }

        public class Data
        {
            public string cardId { get; set; }
            public string itemId { get; set; }
            public string tradeId { get; set; }
            public string tradeState { get; set; }
            public string tradeSkus { get; set; }
            public string resourceId { get; set; }
            public string maskedDefId { get; set; }
            public string definitionId { get; set; }
            public int startingBid { get; set; }
            public int buyNowPrice { get; set; }
            public double billingPrice { get; set; }
            public string billingCurrency { get; set; }
        }
    }
}
