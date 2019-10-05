using System.Data.Entity;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.Database
{
    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class FUTSettingsDatabase : DbContext
    {
        public FUTSettingsDatabase() : base("name=FUTDatabase")
        {
            //DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.CreateIfNotExists();
        }

        public virtual DbSet<FUTSettings> FUTSettings { get; set; }

        public virtual DbSet<FUTListItem> FUTListItems { get; set; }

        public virtual DbSet<FUTProxy> FUTProxys { get; set; }
    }
}
