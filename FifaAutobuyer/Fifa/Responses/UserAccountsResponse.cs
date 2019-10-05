using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class UserAccountsResponse : FUTError
    {
        public UserAccountInfo userAccountInfo { get; set; }
    }
}
