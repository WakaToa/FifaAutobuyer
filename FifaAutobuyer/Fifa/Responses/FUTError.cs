using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public abstract class FUTError
    {
        public string Reason { get; set; }

        public FUTErrorCode Code { get; set; }

        public string Message { get; set; }

        public string Debug { get; set; }

        public string String { get; set; }

        public bool HasError { get { return Code != 0; } }

        public string GetError()
        {
            var ret = "Code: " + Code;
            if (!string.IsNullOrEmpty(Reason))
            {
                ret += " Reason: " + Reason;
            }
            if (!string.IsNullOrEmpty(Message))
            {
                ret += " Message: " + Message;
            }
            if (!string.IsNullOrEmpty(Debug))
            {
                ret += " Debug: " + Debug;
            }
            if (!string.IsNullOrEmpty(String))
            {
                ret += " String: " + String;
            }
            return ret;
        }

    }
}

