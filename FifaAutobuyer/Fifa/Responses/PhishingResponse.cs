using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class PhishingResponse : FUTError
    {
        public int question { get; set; }
        public int attempts { get; set; }
        public int recoverAttempts { get; set; }
        public string token { get; set; }
    }
}
