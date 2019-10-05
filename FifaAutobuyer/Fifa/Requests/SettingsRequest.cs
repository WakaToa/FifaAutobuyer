using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using FifaAutobuyer.Fifa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Requests
{
    public class SettingsRequest : FUTRequestBase, IFUTRequest<SettingsResponse>
    {
        public async Task<SettingsResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Get);
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/settings";
                uriString += "?_=" + Helper.CreateTimestamp();
                var settingsResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
                if (settingsResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new SettingsResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                return await Deserialize<SettingsResponse>(settingsResponseMessage).ConfigureAwait(false);
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new SettingsResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new SettingsResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new SettingsResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }

        public SettingsRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login) { }
    }

}
