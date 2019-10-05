using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Managers
{
    public static class BotManager
    {
        private static List<FUTClient> _futClients;
        public static bool AllAccountsStarted => _futClients.All(x => x.LogicRunningReal);
        private static readonly object _mulingClientsLock = new object();

        public static void Initialize()
        {
            _futClients = new List<FUTClient>();
            var futAccounts = FUTAccountsDatabase.GetFUTAccounts();

            foreach(var account in futAccounts)
            {
                var futClient = new FUTClient(account);
                _futClients.Add(futClient);
            }
        }

        public static void StartAllBots()
        {
            foreach (var acc in _futClients)
            {
                acc.StartLogic();
            }
        }

        public static void StartAllNotRunningBots()
        {
            foreach (var acc in _futClients)
            {
                if (!acc.LogicRunning)
                {
                    acc.StartLogic();
                }

            }
        }

        public static List<FUTClient> GetFutClients()
        {
            if(_futClients == null)
            {
                return new List<FUTClient>();
            }
            return _futClients;
        }

        public static FUTClient GetMostValueableFUTClient(int minCoins)
        {
            lock(_mulingClientsLock)
            {
                if (_futClients == null)
                {
                    return null;
                }
                var validClients = _futClients.Where(x => x.LogicRunningReal && x.LoggedIn && !x.LogicPaused && !x.Muling && !x.PriceChecking && !x.TradepileRoutineRunning).ToList();
                validClients = validClients.Where(x => x.MulingPausedUntil == null || x.MulingPausedUntil <= DateTime.Now).ToList();
                validClients = validClients.Where(x => x.CoinsMuledToday < FUTSettings.Instance.MuleApiMaxSellPerDayPerAccount).ToList();
                validClients = validClients.OrderByDescending(x => x.Coins).ToList();
                validClients = validClients.Where(x => x.Coins >= minCoins).ToList();

                if (validClients.Count <= 0)
                {
                    return null;
                }

                var client = validClients.PickRandom();
                if (client != null)
                {
                    client.Muling = true;
                }
                return client;
            }
        }

        public static void StopAllBots()
        {
            foreach (var acc in _futClients)
            {
                if(acc.LogicRunning)
                {
                    acc.StopLogic();
                }
                
            }
        }

        public static void StartBot(string email)
        {
            var account = _futClients.FirstOrDefault(x => x.FUTAccount.EMail.ToLower() == email.ToLower());
            account?.StartLogic();
        }

        public static List<AuctionInfo> GetTradepileItems()
        {
            if(_futClients == null)
            {
                return new List<AuctionInfo>();
            }

            var ret = new List<AuctionInfo>();
            foreach(var client in _futClients)
            {
                ret.AddRange(client.TradepileItems);
            }
            return ret;
        }

        public static void StopBot(string email)
        {
            var account = _futClients.FirstOrDefault(x => x.FUTAccount.EMail.ToLower() == email.ToLower());
            account?.StopLogic();
        }

        public static void AddBot(string email)
        {
            var futAccount = FUTAccountsDatabase.GetFUTAccountByEMail(email);
            if(futAccount != null && _futClients != null)
            {
                var futClient = new FUTClient(futAccount);
                _futClients.Add(futClient);
            }
        }

        public static void RemoveBot(string email)
        {
            if(_futClients.Contains(email))
            {
                var client = _futClients.FirstOrDefault(x => x.FUTAccount.EMail.ToLower() == email.ToLower());
                if(client != null)
                {
                    StopBot(email);
                    ProxyManager.RemoveProxyCounter(client.FUTProxy);
                    _futClients.Remove(client);
                }
            }
        }

        public static void ResetMuledCoins()
        {
            foreach (var futClient in _futClients)
            {
                futClient.CoinsMuledToday = 0;
            }
        }

        public static void AllocateProxies()
        {
            foreach (var futClient in _futClients)
            {
                futClient.AllocateProxy();
            }
        }
        public static void DeallocateProxies()
        {
            foreach (var futClient in _futClients)
            {
                futClient.DeallocateProxy();
            }
        }
    }
}
