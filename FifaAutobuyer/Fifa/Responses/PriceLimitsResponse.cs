using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class PriceLimitsResponse : FUTError
    {
        public string source { get; set; }
        public int defId { get; set; }
        public long itemId { get; set; }
        public int minPrice { get; set; }
        public int maxPrice { get; set; }
    }
}
