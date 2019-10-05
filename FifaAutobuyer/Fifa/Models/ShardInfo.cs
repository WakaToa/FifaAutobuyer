using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class ShardInfo
    {
        public string shardId { get; set; }
        public string clientFacingIpPort { get; set; }
        public string clientProtocol { get; set; }
        public List<string> platforms { get; set; }
        public List<string> customdata1 { get; set; }
        public List<string> skus { get; set; }
    }
}
