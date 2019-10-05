using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class UserResponse : FUTError
    {
        public int personaId { get; set; }
        public string clubName { get; set; }
        public string clubAbbr { get; set; }
        public int draw { get; set; }
        public int loss { get; set; }
        public int credits { get; set; }
        public BidTokens bidTokens { get; set; }
        public List<Currency> currencies { get; set; }
        public int trophies { get; set; }
        public int won { get; set; }
        public List<Active> actives { get; set; }
        public string established { get; set; }
        public int divisionOffline { get; set; }
        public int divisionOnline { get; set; }
        public string personaName { get; set; }
        public SquadList squadList { get; set; }
        public UnopenedPacks unopenedPacks { get; set; }
        public bool purchased { get; set; }
        public Reliability reliability { get; set; }
        public bool seasonTicket { get; set; }
        public string accountCreatedPlatformName { get; set; }
        public int unassignedPileSize { get; set; }
        public Feature feature { get; set; }
    }
}
