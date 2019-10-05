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
    public class SetNewUserRequest : FUTRequestBase, IFUTRequest<UserResponse>
    {
        public async Task<UserResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Post);
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/user";
                uriString += "?_=" + Helper.CreateTimestamp();
                var content = new StringContent("{\"clubName\":\"" + _hist.clubName + "\",\"clubAbbr\":\"" +  _hist.clubAbbr + "\",\"purchased\":true}");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


                var userResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Post);
                if (userResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new UserResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                return await Deserialize<UserResponse>(userResponseMessage).ConfigureAwait(false);
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new UserResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new UserResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new UserResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }

        private UserHistoricalResponse _hist;

        public SetNewUserRequest(FUTAccount account, RequestPerMinuteManager rpmManager,
            RequestPerMinuteManager rpmManagerSearch, LoginMethod login, UserHistoricalResponse hist)
            : base(account, rpmManager, rpmManagerSearch, login)
        {
            _hist = hist;
        }
    }
}
