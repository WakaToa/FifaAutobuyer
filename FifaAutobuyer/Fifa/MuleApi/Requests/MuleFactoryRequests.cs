using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.MuleApi.Requests
{
    public class MuleFactoryGetPlayerRequest
    {
        public string user { get; set; }
        public string platform { get; set; }
        public int maximumBuyOutPrice { get; set; }
        public long timestamp { get; set; }
        public string hash { get; set; }
    }

    public class MuleFactoryUpdateStatusRequest
    {
        public string user { get; set; }
        public long transactionID { get; set; }
        public string status { get; set; }
        public long timestamp { get; set; }
        public string hash { get; set; }
    }
}
