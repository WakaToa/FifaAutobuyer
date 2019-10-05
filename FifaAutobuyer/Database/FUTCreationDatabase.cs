using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Database.Web;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.Database
{
    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    class FUTCreationDatabase : DbContext
    {
        public FUTCreationDatabase() : base("name=FUTDatabase")
        {
            //DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.CreateIfNotExists();
        }

        public virtual DbSet<FUTAccount> FUTAccounts { get; set; }

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

        public virtual DbSet<FUTSettings> FUTSettings { get; set; }

        public virtual DbSet<FUTListItem> FUTListItems { get; set; }

        public virtual DbSet<FUTProxy> FUTProxies { get; set; }

        public virtual DbSet<WebpanelAccount> WebpanelAccounts { get; set; }

    }
}
