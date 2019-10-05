using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futaccounts")]
    public class FUTAccount
    {
        [Key]
        public string EMail { get; set; }
        public string Password { get; set; }
        public string EMailPassword { get; set; }
        public string SecurityAnswer { get; set; }
        public string BackupCode1 { get; set; }
        public string BackupCode2 { get; set; }
        public string BackupCode3 { get; set; }
        public string GoogleAuthCode { get; set; }

        public FUTPlatform FUTPlatform { get; set; }

        public int FUTProxyID { get; set; }
        public string ProxyString => GetFUTProxy()?.ToString();
        public FUTAccount()
        {
            EMail = "";
            Password = "";
            BackupCode1 = "";
            BackupCode2 = "";
            BackupCode3 = "";
            EMailPassword = "";
            SecurityAnswer = "";
            GoogleAuthCode = "";
            FUTProxyID = -1;
        }

        public FUTProxy GetFUTProxy()
        {
            using (var ctx = new FUTSettingsDatabase())
            {
                return ctx.FUTProxys.FirstOrDefault(x => x.ID == FUTProxyID);
            }
        }
        public void SaveChanges()
        {
            using (var ctx = new FUTAccountsDatabase())
            {
                var pre = ctx.FUTAccounts.FirstOrDefault(x => x.EMail.ToLower() == EMail.ToLower());
                if (pre != null)
                {
                    ctx.Entry(pre).CurrentValues.SetValues(this);
                    ctx.SaveChanges();
                }
            }
        }
    }
}
