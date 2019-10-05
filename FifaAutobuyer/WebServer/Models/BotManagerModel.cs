using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.WebServer.Models
{
    class BotManagerModel : BaseModel
    {
        public int RunningAccounts { get; set; }
        public int TotalAccounts { get; set; }
        public BotManagerModel()
        {
            BotManagerActive = "active";
        }
    }
}
