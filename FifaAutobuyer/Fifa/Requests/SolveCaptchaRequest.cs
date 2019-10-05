using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using FifaAutobuyer.Fifa.Services;

namespace FifaAutobuyer.Fifa.Requests
{
    public class SolveCaptchaRequest : FUTRequestBase, IFUTRequest<bool>
    {
        private string _captchaResult;
        public async Task<bool> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                var httpValidationResponse = await HttpClient.PostAsync(FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/captcha/fun/validate", new StringContent($"{{\"funCaptchaToken\":\"{_captchaResult}\"}}"));
                string validationResponse = await httpValidationResponse.Content.ReadAsStringAsync();

                if (httpValidationResponse.Headers.Contains("Proxy-Authorization"))
                {
                    return false;
                }
                return true;
            }
            catch (HttpRequestException httpEx)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public SolveCaptchaRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login, string captchaResult) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _captchaResult = captchaResult;
        }
    }
}
