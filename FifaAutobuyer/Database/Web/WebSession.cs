using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Database.Web
{
    [Table("webpanelaccounts")]
    public class WebpanelAccount : DbContext
    {
        public WebpanelAccount() : base("name=FUTDatabase")
        {
            //DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.CreateIfNotExists();
        }

        [Key]
        public string Username { get; set; }
        public string Password { get; set; }
        public WebAccessRole Role { get; set; }
    }
    public enum WebAccessRole
    {
        None,
        Viewer,
        Moderator,
        Owner,
        Administrator
    }
}
