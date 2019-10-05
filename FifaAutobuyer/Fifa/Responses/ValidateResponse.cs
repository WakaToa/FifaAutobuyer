using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class ValidateResponse : FUTError
    {
        public string debug { get; set; }
        public string @string { get; set; }
        public string code { get; set; }
        public string reason { get; set; }
        public string token { get; set; }
    }
}
