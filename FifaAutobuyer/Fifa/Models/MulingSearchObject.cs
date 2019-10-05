using FifaAutobuyer.Fifa.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class MulingSearchObject : FUTSearchParameter
    {
        public string Level { get; set; }
        public int MaxBuyNow { get; set; }
        public int MinBuyNow{ get; set; }
        public int AssetID { get; set; }
        public int RevisionID { get; set; } 
        public string Rare { get; set; }

        public int MaxBid { get; set; }
        public int MinBid { get; set; }

        public override string BuildUriString()
        {
            var str = "";
            str += "?type=player";

            if (MaxBuyNow > 0)
            {
                str += "&maxb=" + MaxBuyNow;
            }
            if (MinBuyNow > 0)
            {
                str += "&minb=" + MinBuyNow;
            }
            if (MaxBid > 0)
            {
                str += "&macr=" + MaxBid;
            }
            if (MinBid > 0)
            {
                str += "&micr=" + MinBid;
            }
            if (AssetID > 0)
            {
                var maskedDefId = ResourceIDManager.AssetIDToDefinitionID(AssetID, RevisionID);
                str += "&maskedDefId=" + maskedDefId;
            }
            if (!string.IsNullOrEmpty(Level))
            {
                str += "&lev=" + Level;
            }
            if (!string.IsNullOrEmpty(Rare))
            {
                str += "&rare=" + Rare;
            }

            return str;
        }
    }
}
