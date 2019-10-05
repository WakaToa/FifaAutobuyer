using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Captcha.Json
{
    public class AntiCaptchaResultJson
    {
        public int taskId { get; set; }
        public int errorId { get; set; }
        public string errorCode { get; set; }
        public string errorDescription { get; set; }
        public string status { get; set; }
        public Solution solution { get; set; }
        public string cost { get; set; }
        public string ip { get; set; }
        public int createTime { get; set; }
        public int endTime { get; set; }
        public int solveCount { get; set; }

        public class Solution
        {
            public string token { get; set; }
        }

    }
}
