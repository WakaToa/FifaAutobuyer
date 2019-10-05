using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.MailService
{
    interface IMailClient
    {
        Task<string> GetTwoFactorCode(DateTime codeSent);
        string Username { get; set; }
        string Password { get; set; }
    }
}
