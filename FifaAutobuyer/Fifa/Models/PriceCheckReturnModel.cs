using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public enum PriceCheckReturnReason
    {
        Normal,
        Failed,
        LessThanXXPlayers
    }
    public class PriceCheckReturnResult
    {
        public int CalculatedPrice { get; set; }
        public PriceCheckReturnReason Reason { get; set; }

        public PriceCheckReturnResult(int calculatedPrice, PriceCheckReturnReason reason)
        {
            CalculatedPrice = calculatedPrice;
            Reason = reason;
        }


        public PriceCheckReturnResult()
        {

        }
    }
}
