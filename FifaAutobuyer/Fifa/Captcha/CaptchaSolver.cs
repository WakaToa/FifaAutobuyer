using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Captcha.Json;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Managers;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.Captcha
{
    public class CaptchaSolver
    {
        public const string WebsiteURL = "https://www.easports.com/de/fifa/ultimate-team/web-app/";
        public const string WebsiteKey = "A4EECF77-AC87-8C8D-5754-BF882F72063B";

        private HttpClient httpClient;
        private FUTProxy _preProxy;

        public CaptchaSolver(FUTProxy proxy)
        {
            httpClient = new HttpClient(new HttpClientHandler());
            _preProxy = proxy;
        }

        private string GetProxy()
        {
            //if (_preProxy != null)
            //{
            //    return $"{_preProxy.Host}:{_preProxy.Port}:{_preProxy.Username}:{_preProxy.Password}";
            //}
            var proxys = AppSettingsManager.GetAntiCaptchaProxys();

            return proxys.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        }

        public async Task<AntiCaptchaResultJson> PrepareAntiCaptchaAsync()
        {
            var proxy = GetProxy();


            var createTask = JsonConvert.SerializeObject(new AntiCaptchaCreateTaskJson()
            {
                clientKey = AppSettingsManager.GetAntiCaptchaKey(),
                task = new AntiCaptchaCreateTaskJson.Task()
                {
                    type = "FunCaptchaTask",
                    websiteURL = WebsiteURL,
                    websitePublicKey = WebsiteKey,
                    proxyType = "http",
                    proxyAddress = proxy.Split(':')[0],
                    proxyPort = int.Parse(proxy.Split(':')[1]),
                    proxyLogin = proxy.Split(':')[2],
                    proxyPassword = proxy.Split(':')[3],
                    userAgent = HttpClientEx.UserAgent
                }
            });

            try
            {
                var result = await httpClient.PostAsync("https://api.anti-captcha.com/createTask", new StringContent(createTask, Encoding.UTF8, "application/json"));
                var resultString = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AntiCaptchaResultJson>(resultString);
            }
            catch (Exception e)
            {
                return new AntiCaptchaResultJson() { errorCode = "999", errorDescription = e.Message };
            }

        }

        public async Task<AntiCaptchaResultJson> CheckStatus(int taskId)
        {
            var getTask = "{\"clientKey\":\"" + AppSettingsManager.GetAntiCaptchaKey() + "\",\"taskId\": " + taskId + "}";

            try
            {
                var result = await httpClient.PostAsync("https://api.anti-captcha.com/getTaskResult", new StringContent(getTask, Encoding.UTF8, "application/json"));
                var resultString = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AntiCaptchaResultJson>(resultString);
            }
            catch (Exception e)
            {
                return new AntiCaptchaResultJson() { errorId = 999, errorDescription = e.Message };
            }

        }

        public async Task<AntiCaptchaResultJson> DoAntiCaptcha()
        {
            for (var i = 0; i <= 2; i++)
            {
                var task = await PrepareAntiCaptchaAsync();

                if (task.errorId != 0)
                {
                    await Task.Delay(10000);
                    continue;
                }
                while (true)
                {
                    var result = await CheckStatus(task.taskId);
                    if (result.status == "processing")
                    {
                        await Task.Delay(10000);
                    }
                    else
                    {
                        if (result.errorId == 0)
                        {
                            return result;
                        }
                        break;
                    }
                }
            }
            return new AntiCaptchaResultJson() { errorId = 999, errorDescription = "AntiCaptcha failed 3 times" };
        }
    }
}
