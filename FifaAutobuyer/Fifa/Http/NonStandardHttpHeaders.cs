﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Http
{
    public class NonStandardHttpHeaders
    {
        public const string MethodOverride = "X-HTTP-Method-Override";

        public const string PhishingToken = "X-UT-PHISHING-TOKEN";

        public const string RequestedWith = "X-Requested-With";

        public const string NucleusId = "Easw-Session-Data-Nucleus-Id";

        public const string EmbedError = "X-UT-Embed-Error";

        public const string Route = "X-UT-Route";

        public const string SessionId = "X-UT-SID";

        public const string PowSessionId = "X-POW-SID";
    }
}
