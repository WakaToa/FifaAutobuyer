using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.WebServer.Models
{
    class ItemStatisticModel : BaseModel
    {
        public List<SimpleItemStatistic> ItemStatistic { get; set; }

        public ItemStatisticModel()
        {
            ItemStatisticActive = "active";
        }
    }
}
