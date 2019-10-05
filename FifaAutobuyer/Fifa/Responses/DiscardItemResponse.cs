using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{

    public class Item
    {
        public long id { get; set; }
    }

    public class DiscardItemResponse : FUTError
    {
        public List<Item> items { get; set; }
        public int totalCredits { get; set; }
    }
}
