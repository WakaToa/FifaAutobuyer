using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class TransferMarketResponse : FUTError
    {
        public object errorState { get; set; }
        public int credits { get; set; }
        public List<AuctionInfo> auctionInfo { get; set; }
        public object duplicateItemIdList { get; set; }
        public BidTokens bidTokens { get; set; }
        public List<Currency> currencies { get; set; }
    }
}
