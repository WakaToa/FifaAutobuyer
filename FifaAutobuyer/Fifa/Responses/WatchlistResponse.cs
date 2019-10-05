using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.Fifa.Responses
{
    public class WatchlistResponse : FUTError
    {
        public int total { get; set; }
        public int credits { get; set; }
        public List<AuctionInfo> auctionInfo { get; set; }
    }
}
