using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Database
{
    public class FUTPlatform
    {
        public string SKU { get; set; }
        public string GameSKU { get; set; }
        public string PersonaPlatform { get; set; }
        public string Route { get; set; }

        public FUTPlatform()
        {
            SKU = "";
            GameSKU = "";
            PersonaPlatform = "";
            Route = "";
        }
    }
}
