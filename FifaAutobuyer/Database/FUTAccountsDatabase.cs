namespace FifaAutobuyer.Database
{
    using Fifa.Database;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Linq;
    using System;

    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class FUTAccountsDatabase : DbContext
    {
        public FUTAccountsDatabase() : base("name=FUTDatabase")
        {
            //DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.CreateIfNotExists();
        }
        public virtual DbSet<FUTAccount> FUTAccounts { get; set; }

        private static object _accountsLock = new object();
        public static void AddFUTAccount(FUTAccount account)
        {
            lock(_accountsLock)
            {
                using (var context = new FUTAccountsDatabase())
                {
                    context.FUTAccounts.Add(account);
                    context.SaveChanges();
                }
            }
        }

        public static void RemoveFUTAccountByEMail(string account)
        {
            lock(_accountsLock)
            {
                using (var context = new FUTAccountsDatabase())
                {
                    var accFromDatabase = context.FUTAccounts.FirstOrDefault(x => x.EMail == account);
                    if (accFromDatabase != null)
                    {
                        context.FUTAccounts.Remove(accFromDatabase);
                        context.SaveChanges();
                    }
                }
            }
        }

        public static List<FUTAccount> GetFUTAccounts()
        {
            lock(_accountsLock)
            {
                using (var context = new FUTAccountsDatabase())
                {
                    return context.FUTAccounts.ToList();
                }
            }
        }

        public static FUTAccount GetFUTAccountByEMail(string email)
        {
            lock(_accountsLock)
            {
                using (var context = new FUTAccountsDatabase())
                {
                    return context.FUTAccounts.FirstOrDefault(x => x.EMail.ToLower() == email.ToLower());
                }
            }
        }
    }
}