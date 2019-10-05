using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Services.GoogleAuthenticator
{
    public class QuickEAAuthenticator
    {
        public static string GenerateAuthCode(string secret)
        {
            var authenticator = new TimeAuthenticator();
            return authenticator.GetCode(secret);
        }
    }
}
