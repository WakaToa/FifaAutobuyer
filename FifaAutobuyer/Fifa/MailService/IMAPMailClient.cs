using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using S22.Imap;

namespace FifaAutobuyer.Fifa.MailService
{
    public class IMAPMailClient : IMailClient
    {
        public IMAPMailClient(string user, string pass, string host, int port, bool useSsl)
        {
            Username = user;
            Password = pass;
            _hostName = host;
            _port = port;
            _useSsl = useSsl;
        }
        private string GetTwoFactorCodeInternal()
        {
            var code = "";
            try
            {

                using (var client = new ImapClient(_hostName, _port, Username, Password, AuthMethod.Auto, _useSsl, (sender, certificate, chain, errors) => true))
                {
                    List<uint> uids = client.Search(SearchCondition.To(Username).And(SearchCondition.From("ea.com"))).ToList();
                    var mails = client.GetMessages(uids, FetchOptions.Normal);

                    foreach (var msg in mails)
                    {
                        if (msg == null)
                        {
                            continue;
                        }
                        if (msg.From.Address.Contains("ea.com") && Regex.IsMatch(msg.Subject, "([0-9]+)") && ((DateTime)msg.Date()).ToUniversalTime() > _codeSent.ToUniversalTime())
                        {
                            var mailBody = msg.Subject;
                            code = Regex.Match(mailBody, "([0-9]+)").Groups[1].Value;
                            break;
                        }
                    }
                    if (uids.Count > 0)
                    {
                        client.DeleteMessages(uids);
                    }

                }
            }
#pragma warning disable CS0168
            catch (Exception e)
            {
                code = "EXC" + e;
            }
#pragma warning restore CS0168
            return code;
        }

        private DateTime _codeSent;
        public async Task<string> GetTwoFactorCode(DateTime codeSent)
        {
            var ret = "";
            _codeSent = codeSent;
            await Task.Run(() =>
            {
                ret = GetTwoFactorCodeInternal();
            }).ConfigureAwait(false);
            return ret;
        }

        public string Username { get; set; }
        public string Password { get; set; }

        private string _hostName;
        private int _port;
        private bool _useSsl;
    }
}
