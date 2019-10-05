using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.WebServer.Models
{
    public class AccountManagerModel : BaseModel
    {
        public AccountManagerModel()
        {
            AccountManagerActive = "active";
        }

        public List<FUTAccount> Accounts { get; set; }
    }
}
