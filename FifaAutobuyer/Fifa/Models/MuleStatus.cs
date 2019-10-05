using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class MuleStatus : ICloneable
    {
        public string EMail { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }

        public MuleStatus(string email, string message)
        {
            EMail = email;
            Message = message;
            DateTime = DateTime.Now;
        }

        public object Clone()
        {
            var status = new MuleStatus(EMail, Message);
            status.DateTime = DateTime;
            return status;

        }
    }
}
