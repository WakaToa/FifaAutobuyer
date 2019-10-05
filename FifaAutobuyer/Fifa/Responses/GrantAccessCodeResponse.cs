using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class GrantAccessCodeResponse : FUTError
    {
        public string access_token { get; set; }
        public object origin_access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public object origin_refresh_token { get; set; }
        public object id_token { get; set; }
        public object origin_id_token { get; set; }
        public object additional_access_tokens { get; set; }
    }
}
