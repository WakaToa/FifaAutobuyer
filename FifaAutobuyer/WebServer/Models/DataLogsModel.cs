using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.WebServer.Models
{
    public class DataLogsModel : BaseModel
    {
        public string Title { get; set; }
        public string FooterPrevious { get; set; }
        public string FooterNext { get; set; }
        public List<SingleDataLog> Logs { get; set; }

        public class SingleDataLog
        {
            public int ID { get; set; }
            public string Account { get; set; }
            public string Data { get; set; }
            public string Timestamp { get; set; }
        }
    }
}
