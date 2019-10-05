using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Database.Web
{
    public class WebSessionsDatabase : System.Data.Entity.DbContext
    {
        public WebSessionsDatabase() : base("name=FUTDatabase")
        {
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.ProxyCreationEnabled = true;
        }

        public virtual DbSet<WebpanelAccount> WebSessions { get; set; }

        public static List<WebpanelAccount> GetWebSessions()
        {
            using (var context = new WebSessionsDatabase())
            {
                return context.WebSessions.ToList();
            }
        }

        public static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper().Substring(0,32);
        }
    }
}
