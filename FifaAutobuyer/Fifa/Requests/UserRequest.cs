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
    public class UserRequest : FUTRequestBase, IFUTRequest<UserResponse>
    {
        public async Task<UserResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Get);
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/user";
                uriString += "?_=" + Helper.CreateTimestamp();
                var userResponseMessage = await HttpClient.GetAsync(uriString).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
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

        public UserRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login) { }
    }
}
