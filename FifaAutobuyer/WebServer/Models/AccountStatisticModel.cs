using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa;
using FifaAutobuyer.Fifa.Extensions;

namespace FifaAutobuyer.WebServer.Models
{
    class AccountStatisticModel : BaseModel
    {
        public List<Pair<FUTClient, long>> FUTClients { get; set; }

        public AccountStatisticModel()
        {
            AccountStatisticActive = "active";
        }
    }
}
