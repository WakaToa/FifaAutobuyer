using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{

    public class LoginResponse : FUTError
    {
        public string NucleusID { get; set; }

        public string NucleusName { get; set; }

        public string PersonaID { get; set; }

        public string SessionID { get; set; }

        public string PhishingToken { get; set; }
    }
}
