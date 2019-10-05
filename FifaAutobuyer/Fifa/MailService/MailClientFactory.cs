using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using FifaAutobuyer.Fifa.Extensions;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.MailService
{
    class MailService
    {
        public string Type { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }
    class MailClientFactory
    {
        private static List<MailService> _knownMailServices;

        public static void Initialize()
        {
            _knownMailServices = new List<MailService>();
            if (File.Exists("mailservice.json"))
            {
                _knownMailServices = JsonConvert.DeserializeObject<List<MailService>>(File.ReadAllText("mailservice.json"));
            }
        }
        public static IMailClient Create(string username, string password)
        {
            var mailProvider = username.Split('@')[1].ToLower();
            var detectedType = _knownMailServices.FirstOrDefault(x => x.Type == mailProvider);
            if(detectedType == null)
            {
                return null;
            }
            IMailClient ret = new IMAPMailClient(username, password, detectedType.Host, detectedType.Port, detectedType.UseSSL);
            return ret;
        }
    }
}
