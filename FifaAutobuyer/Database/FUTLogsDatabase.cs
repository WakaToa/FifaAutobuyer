using System.Data.SqlTypes;
using System.Threading.Tasks;

namespace FifaAutobuyer.Database
{
    using Fifa.Database;
    using Fifa.Managers;
    using Fifa.Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;

    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class FUTLogsDatabase : DbContext
    {
        public FUTLogsDatabase() : base("name=FUTDatabase")
        {
            //DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.CreateIfNotExists();
        }

        public virtual DbSet<FUTCoins> FUTCoins { get; set; }
        public virtual DbSet<FUTLog> FUTLogs { get; set; }
        public virtual DbSet<FUTBuy> FUTBuys { get; set; }
        public virtual DbSet<FUTSell> FUTSells { get; set; }
        public virtual DbSet<FUTBotLog> FUTBotLogs { get; set; }
        public virtual DbSet<FUTExceptionLog> FUTExceptionLogs { get; set; }
        public virtual DbSet<FUTAccountStatistic> FUTAccountStatistics { get; set; }
        public virtual DbSet<FUTPriceCheck> FUTPriceChecks { get; set; }
        public virtual DbSet<FUTItemProfit> FUTItemProfits { get; set; }
        public virtual DbSet<FUTNotification> FUTNotifications { get; set; }
        public virtual DbSet<FUTBotStatistics> FUTBotStatistics { get; set; }
        public virtual DbSet<FUTMuleApiStatistic> FUTMuleApiStatistics { get; set; }

        public static void UpdateCoinsByFUTAccount(FUTAccount account, int coins)
        {
            using (var context = new FUTLogsDatabase())
            {
                var lastSession = context.FUTCoins.FirstOrDefault(x => x.EMail.ToLower() == account.EMail.ToLower());
                if (lastSession != null)
                {
                    lastSession.Coins = coins;
                }
                else
                {
                    var coinsNew = new FUTCoins
                    {
                        EMail = account.EMail,
                        Coins = coins
                    };
                    context.FUTCoins.Add(coinsNew);
                }
                context.SaveChanges();
            }
        }

        public static bool CheckNextPageFUTBuysLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                var r = context.FUTBuys.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
                return r.Count > 0;
            }
        }

        public static bool CheckPreviousPageFUTBuysLogs(int from, int take)
        {
            if (from == 0)
            {
                return false;
            }
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBuys.OrderByDescending(x => x.ID).Skip(from - 1).Take(take).Any();
            }
        }

        public static bool CheckNextPageFUTSellsLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                var r = context.FUTSells.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
                return r.Count > 0;
            }
        }

        public static bool CheckPreviousPageFUTSellsLogs(int from, int take)
        {
            if (from == 0)
            {
                return false;
            }
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTSells.OrderByDescending(x => x.ID).Skip(from - 1).Take(take).Any();
            }
        }

        public static bool CheckNextPageFUTBotLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                var r = context.FUTBotLogs.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
                return r.Count > 0;
            }
        }

        public static bool CheckPreviousPageFUTBotLogs(int from, int take)
        {
            if (from == 0)
            {
                return false;
            }
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBotLogs.OrderByDescending(x => x.ID).Skip(from - 1).Take(take).Any();
            }
        }

        public static bool CheckNextPageFUTProfitLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                var r = context.FUTItemProfits.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
                return r.Count > 0;
            }
        }

        public static bool CheckPreviousPageFUTProfitLogs(int from, int take)
        {
            if (from == 0)
            {
                return false;
            }
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTItemProfits.OrderByDescending(x => x.ID).Skip(from - 1).Take(take).Any();
            }
        }

        public static List<FUTCoins> GetFUTCoins()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTCoins.ToList();
            }
        }
        public static FUTCoins GetFUTCoinsByEMail(string email)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTCoins.FirstOrDefault(x => x.EMail == email);
            }
        }

        public static List<FUTBuy> GetFUTBuys()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBuys.ToList();
            }
        }
        public static List<FUTBuy> GetFUTBuysByAssetID(int assetID, int revID)
        {
            using (var context = new FUTLogsDatabase())
            {
                var allBuys = context.FUTBuys.ToList();
                return allBuys.Where(x => x.AssetID == assetID && x.RevisionID == revID).ToList();
            }
        }
        public static List<FUTBuy> GetFUTBuysByAssetID(int assetID, int revID, int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                var allSells = context.FUTBuys.ToList();
                return allSells.Where(x => x.AssetID == assetID && x.RevisionID == revID).OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }
        public static int GetFUTBuysCount()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBuys.Count();
            }
        }

        public static List<FUTBuy> GetFUTBuys(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBuys.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }
        public static List<FUTBuy> GetFUTBuys(int from, int take, FUTBuyBidType type)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBuys.Where(x => x.Type == type).OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }
        public static int GetFUTBuysCountLast24Hours(string email)
        {
            var now = Helper.CreateTimestamp();
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBuys.Count(x => x.EMail.ToLower() == email.ToLower() && (now - x.Timestamp) <= 86400000);
            }
        }

        public static List<FUTSell> GetFUTSells()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTSells.ToList();
            }
        }
        public static List<FUTSell> GetFUTSellsByAssetID(int assetID, int revID)
        {
            using (var context = new FUTLogsDatabase())
            {
                var allSells = context.FUTSells.ToList();
                return allSells.Where(x => x.AssetID == assetID && x.RevisionID == revID).ToList();
            }
        }
        public static List<FUTSell> GetFUTSellsByAssetIDSite(int assetID, int revID, int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                var allSells = context.FUTSells.ToList();
                return allSells.Where(x => x.AssetID == assetID && x.RevisionID == revID).OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }
        public static int GetFUTSellsCount()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTSells.Count();
            }
        }

        public static List<FUTSell> GetFUTSells(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTSells.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }
        public static List<FUTSell> GetFUTSells(int from, int take, FUTBuyBidType type)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTSells.Where(x => x.Type == type).OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }

        public static List<FUTBotLog> GetFUTBotLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBotLogs.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }
        public static List<FUTBotLog> GetFUTBotLogsByEMail(string email, int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBotLogs.Where(x=> x.EMail.ToLower() == email.ToLower()).OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }

        public static List<FUTExceptionLog> GetFUTExceptionLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTExceptionLogs.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
            }
        }

        public static bool CheckNextPageFUTExceptionLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                var r = context.FUTExceptionLogs.OrderByDescending(x => x.ID).Skip(from).Take(take).ToList();
                return r.Count > 0;
            }
        }

        public static bool CheckPreviousPageFUTExceptionLogs(int from, int take)
        {
            if (from == 0)
            {
                return false;
            }
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTExceptionLogs.OrderByDescending(x => x.ID).Skip(from - 1).Take(take).Any();
            }
        }

        public static List<FUTNotification> GetFUTNotifications()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTNotifications.ToList();
            }
        }

        public static int GetFUTNotificationCount()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTNotifications.Count();
            }
        }

        public static List<FUTBotStatistics> GetFUTBotStatisticsLast24Hours()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTBotStatistics.OrderByDescending(x => x.ID).Where(x => x.TotalTradepileItems >= 1).Take(48).ToList();
            }
        }

        public static List<FUTItemProfit> GetFUTProfitLogsLast24Hours()
        {
            var now = Helper.CreateTimestamp();
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTItemProfits.OrderByDescending(x => x.ID).Where(x => (now - x.SellTimestamp) <= 86400000).ToList();
            }
        }

        public static void InsertFUTBuy(FUTBuy buy)
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTBuys.Add(buy);
                context.SaveChanges();
            }
        }
        public static void InsertFUTLog(FUTLog log)
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTLogs.Add(log);
                context.SaveChanges();
            }
        }
        public static void InsertFUTSell(FUTSell sell)
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTSells.Add(sell);
                context.SaveChanges();
            }
        }
        public static void InsertFUTBotLog(FUTBotLog log)
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTBotLogs.Add(log);
                context.SaveChanges();
            }
        }
        public static void InsertFUTExceptionLog(FUTExceptionLog log)
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTExceptionLogs.Add(log);
                context.SaveChanges();
            }
        }
        public static void AddFUTNotification(string from, string data)
        {
            var logData = new FUTNotification(from, data);
            using (var context = new FUTLogsDatabase())
            {
                context.FUTNotifications.Add(logData);
                context.SaveChanges();
            }
        }
        public static void InsertFUTPriceCheck(FUTPriceCheck priceCheck)
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTPriceChecks.Add(priceCheck);
                context.SaveChanges();
            }
        }
        public static void InsertFUTBotStatistics(FUTBotStatistics log)
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTBotStatistics.Add(log);
                context.SaveChanges();
            }
        }

        private static readonly object _removeFUTNotificationLock = new object();
        public static void RemoveFUTNotification(int id)
        {
            lock(_removeFUTNotificationLock)
            {
                using (var context = new FUTLogsDatabase())
                {
                    var data = context.FUTNotifications.FirstOrDefault(x => x.ID == id);
                    if (data != null)
                    {
                        context.FUTNotifications.Remove(data);
                        context.SaveChanges();
                    }
                    
                }
            }

        }

        public static void RemoveFUTBuy(long id)
        {
            using (var context = new FUTLogsDatabase())
            {
                var entity = context.FUTBuys.FirstOrDefault(x => x.ID == id);
                if(entity != null)
                {
                    context.FUTBuys.Remove(entity);
                    context.SaveChanges();
                }
            }
        }

        public static void RemoveFUTSell(long id)
        {
            using (var context = new FUTLogsDatabase())
            {
                var entity = context.FUTSells.FirstOrDefault(x => x.ID == id);
                if (entity != null)
                {
                    context.FUTSells.Remove(entity);
                    context.SaveChanges();
                }
            }
        }

        public static void ResetFUTSells()
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTSells.RemoveRange(context.FUTSells);
                context.SaveChanges();
            }
        }

        public static void ResetFUTBuys()
        {
            using (var context = new FUTLogsDatabase())
            {
                context.FUTBuys.RemoveRange(context.FUTBuys);
                context.SaveChanges();
            }
        }

        public static void InsertFUTItemProfit(FUTItemProfit item)
        {
            using (var context = new FUTLogsDatabase())
            {
                var dbItem = context.FUTItemProfits.FirstOrDefault(x => x.ItemID == item.ItemID);
                if (dbItem == null)
                {
                    context.FUTItemProfits.Add(item);
                    context.SaveChanges();
                }
                else
                {
                    context.Entry(dbItem).CurrentValues.SetValues(item);
                    context.SaveChanges();
                }
            }
        }

        public static void UpdateFUTItemProfitByItemID(long itemID, int sellPrice)
        {
            using (var context = new FUTLogsDatabase())
            {
                var dbItem = context.FUTItemProfits.FirstOrDefault(x => x.ItemID == itemID);
                if (dbItem == null) return;
                dbItem.SellPrice = sellPrice;
                dbItem.Profit = (int)(sellPrice * 0.95) - dbItem.BuyPrice;
                dbItem.SellTimestamp = Helper.CreateTimestamp();
                context.SaveChanges();
            }
        }

        public static FUTItemProfit GetFUTItemProfitByItemID(long itemID)
        {
            using (var context = new FUTLogsDatabase())
            {
                var dbItem = context.FUTItemProfits.FirstOrDefault(x => x.ItemID == itemID);
                return dbItem;
            }
        }

        public static List<FUTItemProfit> GetFUTProfitLogs(int from, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTItemProfits.Where(x => x.SellTimestamp != 0).OrderByDescending(x => x.SellTimestamp).Skip(from).Take(take).ToList();
            }
        }
        public static List<FUTItemProfit> GetFUTProfitLogs(int from, int take, FUTBuyBidType type)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTItemProfits.Where(x => x.SellTimestamp != 0 && x.Type == type).OrderByDescending(x => x.SellTimestamp).Skip(from).Take(take).ToList();
            }
        }

        public static List<FUTItemProfit> GetFUTProfitLogs()
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTItemProfits.ToList();
            }
        }

        public static List<FUTItemProfit> GetFUTProfitLogs(int from, int take, int assetID, int revID, FUTBuyBidType type)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTItemProfits.Where(x => x.SellTimestamp != 0 && x.AssetID == assetID && x.RevisionID == revID && x.Type == type).OrderByDescending(x => x.SellTimestamp).Skip(from).Take(take).ToList();
            }
        }

        public static List<FUTItemProfit> GetFUTProfitLogs(int from, int take, int assetID, int revID)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTItemProfits.Where(x => x.SellTimestamp != 0 && x.AssetID == assetID && x.RevisionID == revID).OrderByDescending(x => x.SellTimestamp).Skip(from).Take(take).ToList();
            }
        }

        public static List<FUTPriceCheck> GetFUTPriceChecksByAssetIDRevisionID(int assetID, int revID, int take)
        {
            using (var context = new FUTLogsDatabase())
            {
                return context.FUTPriceChecks.Where(x => x.AssetID == assetID && x.RevisionID == revID).OrderByDescending(x => x.ID).Take(take).ToList();
            }
        }
    }
}