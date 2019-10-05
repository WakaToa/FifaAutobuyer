using FifaAutobuyer.Fifa.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Extensions
{
    public static class DatabaseExtensions
    {
        public static bool Contains(this List<FUTAccount> list , string email)
        {
            foreach(var b in list)
            {
                if(b.EMail.ToLower() == email.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains(this List<FUTClient> list, string email)
        {
            foreach (var b in list)
            {
                if (b.FUTAccount.EMail.ToLower() == email.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
