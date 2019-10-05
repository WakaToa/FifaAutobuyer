using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class ItemData
    {
        public long id { get; set; }
        public int timestamp { get; set; }
        public string formation { get; set; }
        public bool untradeable { get; set; }
        public int assetId { get; set; }
        public int rating { get; set; }
        public string itemType { get; set; }
        public int resourceId { get; set; }
        public int owners { get; set; }
        public int discardValue { get; set; }
        public string itemState { get; set; }
        public int cardsubtypeid { get; set; }
        public int lastSalePrice { get; set; }
        public int morale { get; set; }
        public int fitness { get; set; }
        public string injuryType { get; set; }
        public int injuryGames { get; set; }
        public string preferredPosition { get; set; }
        public List<object> statsList { get; set; }
        public List<object> lifetimeStats { get; set; }
        public int training { get; set; }
        public int contract { get; set; }
        public int suspension { get; set; }
        public List<AttributeList> attributeList { get; set; }
        public int teamid { get; set; }
        public int rareflag { get; set; }
        public int playStyle { get; set; }
        public int leagueId { get; set; }
        public int loyaltyBonus { get; set; }
        public int pile { get; set; }
        public int nation { get; set; }
    }
}
