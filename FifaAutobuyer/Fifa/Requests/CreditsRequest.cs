using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Models;
using System.Net.Sockets;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.Database.Settings;

namespace FifaAutobuyer.Fifa.Requests
{
    public class CreditsRequest : FUTRequestBase, IFUTRequest<CreditsResponse>
    {
        public async Task<CreditsResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Get);
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/user/credits";
                uriString += "?_=" + Helper.CreateTimestamp();
                var creditsResponseMessage = await HttpClient.GetAsync(uriString).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
                if (creditsResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new CreditsResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                var result = await Deserialize<CreditsResponse>(creditsResponseMessage).ConfigureAwait(false);
                return result;
            }
            catch(HttpRequestException httpEx)
            {
                if(httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new CreditsResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new CreditsResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch(Exception e)
            {
                var resp = new CreditsResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }
        }

        public CreditsRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login) { }
    }
}
