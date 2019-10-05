using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.WebServer.Models
{
    public class ProxyManagerModel : BaseModel
    {
        public ProxyManagerModel()
        {
            ProxyManagerActive = "active";
        }
        public List<FUTProxy> Proxies { get; set; }
    }
}