using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa;
using FifaAutobuyer.Fifa.MuleApi;

namespace FifaAutobuyer.WebServer.Models
{
    class MuleSessionModel : BaseModel
    {
        public MuleClient MuleClient { get; set; }

        public MuleSessionModel()
        {
            MuleManagerActive = "active";
        }
    }
}
