using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.WebServer.Models
{
    class NotificationCenterModel : BaseModel
    {
        public List<Tuple<FUTNotification, string>> Notifications { get; set; }

        public NotificationCenterModel()
        {
            NotificationCenterActive = "active";
        }
    }
}
