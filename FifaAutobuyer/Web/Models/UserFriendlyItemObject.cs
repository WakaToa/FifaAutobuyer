using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Web.Models
{
    public class UserFriendlyItemObject
    {
        public string Name { get; set; }
        public int Rating { get; set; }
        public int AssetID { get; set; }
        public int RevisionID { get; set; }
        public FUTSearchParameterType Type { get; set; }
    }
}
