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
    public class UserMassInfoRequest : FUTRequestBase, IFUTRequest<UserMassInfoResponse>
    {
        public async Task<UserMassInfoResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Get);
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/usermassinfo";
                uriString += "?_=" + Helper.CreateTimestamp();
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


                var userResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
                if (userResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new UserMassInfoResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                return await Deserialize<UserMassInfoResponse>(userResponseMessage).ConfigureAwait(false);
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new UserMassInfoResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new UserMassInfoResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new UserMassInfoResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }

        public UserMassInfoRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod loginMethod) : base(account, rpmManager, rpmManagerSearch, loginMethod)
        {
        }
    }
}
