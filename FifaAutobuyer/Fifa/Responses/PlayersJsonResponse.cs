using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class PlayersJsonResponse
    {
        public List<SimpleSearchItemModel> Players { get; set; }
        public List<SimpleSearchItemModel> LegendsPlayers { get; set; }

        public List<SimpleSearchItemModel> AllPlayers()
        {
            var copy = new List<SimpleSearchItemModel>(Players);
            copy.AddRange(LegendsPlayers);
            return copy;
        }
    }
}
