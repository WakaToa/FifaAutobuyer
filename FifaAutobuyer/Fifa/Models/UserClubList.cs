using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class UserClubList
    {
        public string year { get; set; }
        public int assetId { get; set; }
        public int teamId { get; set; }
        public int lastAccessTime { get; set; }
        public string platform { get; set; }
        public string clubName { get; set; }
        public string clubAbbr { get; set; }
        public int established { get; set; }
        public int divisionOnline { get; set; }
        public int badgeId { get; set; }
        public Dictionary<string,long> skuAccessList { get; set; }
    }
}
