﻿using System;
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
    public class UserHistoricalRequest : FUTRequestBase, IFUTRequest<UserHistoricalResponse>
    {
        public async Task<UserHistoricalResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Get);
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/user/historical";
                uriString += "?_=" + Helper.CreateTimestamp();
                var creditsResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
                if (creditsResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new UserHistoricalResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                var result = await Deserialize<UserHistoricalResponse>(creditsResponseMessage);
                return result;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new UserHistoricalResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new UserHistoricalResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new UserHistoricalResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }

        public UserHistoricalRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login) { }
    }
}
