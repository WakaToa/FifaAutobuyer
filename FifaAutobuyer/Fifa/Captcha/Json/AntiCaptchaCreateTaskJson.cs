using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Captcha.Json
{
    public class AntiCaptchaCreateTaskJson
    {
        public string clientKey { get; set; }
        public Task task { get; set; }
        public class Task
        {
            public string type { get; set; }
            public string websiteURL { get; set; }
            public string websitePublicKey { get; set; }
            public string proxyType { get; set; }
            public string proxyAddress { get; set; }
            public int proxyPort { get; set; }
            public string proxyLogin { get; set; }
            public string proxyPassword { get; set; }
            public string userAgent { get; set; }
        }
    }
}
