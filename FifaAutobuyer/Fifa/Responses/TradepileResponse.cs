using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class TradepileResponse : FUTError
    {
        public object errorState { get; set; }
        public int credits { get; set; }
        public List<AuctionInfo> auctionInfo { get; set; }
        public object duplicateItemIdList { get; set; }
        public BidTokens bidTokens { get; set; }
        public object currencies { get; set; }
    }
}
