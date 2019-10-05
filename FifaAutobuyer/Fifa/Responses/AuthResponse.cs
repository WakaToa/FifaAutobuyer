using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class AuthResponse : FUTError
    {
        public string protocol { get; set; }
        public string ipPort { get; set; }
        public string serverTime { get; set; }
        public string lastOnlineTime { get; set; }
        public string sid { get; set; }
    }
}
