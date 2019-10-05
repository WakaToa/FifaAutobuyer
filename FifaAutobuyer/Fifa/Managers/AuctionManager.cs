using FifaAutobuyer.Fifa.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Managers
{
    public class AuctionManager
    {

        private static ConcurrentDictionary<long, DateTime> _tradeIDs = new ConcurrentDictionary<long, DateTime>();
        private static object _lock = new object();
        public static bool CanBuyAuction(long auctionID)
        {
            lock(_lock)
            {
                if (_tradeIDs.ContainsKey(auctionID))
                {
                    return false;
                }
                _tradeIDs.TryAdd(auctionID, DateTime.Now);
                return true;
            }
        }


        public static void RemoveOldAuctions()
        {
            List<long> remove = new List<long>();
            foreach (var auction in _tradeIDs)
            {
                if(DateTime.Now.Subtract(auction.Value).TotalHours >= 1)
                {
                    remove.Add(auction.Key);
                }
            }


            foreach (var auction in remove)
            {
                DateTime outt;
                _tradeIDs.TryRemove(auction, out outt);
            }
        }
    }
}
