using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using FifaAutobuyer.Fifa;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.MuleApi;

namespace FifaAutobuyer.WebServer.Models
{
    public class MuleManagerModel : BaseModel
    {
        public List<MuleClient> Clients { get; set; }

        public MuleManagerModel()
        {
            MuleManagerActive = "active";
        }
    }
}
