using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.WebServer.Models
{
    public class BuysSellsModel : BaseModel
    {
        public string Title { get; set; }
        public string PriceString { get; set; }
        public string FooterPrevious { get; set; }
        public string FooterNext { get; set; }
        public List<SingleDataLog> Logs { get; set; }
        public string LogType { get; set; }

        public class SingleDataLog
        {
            public long ID { get; set; }
            public string TimestampString { get; set; }
            public long TradeID { get; set; }
            public string ItemName { get; set; }
            public int ResourceID { get; set; }
            public int RevisionID { get; set; }
            public int Price { get; set; }
        }
    }
}
