using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class CreditsResponse : FUTError
    {
        public int credits { get; set; }
        public BidTokens bidTokens { get; set; }
        public List<Currency> currencies { get; set; }
        public UnopenedPacks unopenedPacks { get; set; }
    }
}
