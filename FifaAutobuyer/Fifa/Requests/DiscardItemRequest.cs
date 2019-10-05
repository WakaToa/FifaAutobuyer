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
    public class DiscardItemRequest : FUTRequestBase, IFUTRequest<DiscardItemResponse>
    {
        public async Task<DiscardItemResponse> PerformRequestAsync()
        {
            try
            {
                if (!_muling)
                {
                    await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(2500);
                }
                AddMethodOverrideHeader(HttpMethod.Delete);
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/item/" + _item;
                uriString += "?_=" + Helper.CreateTimestamp();
                var deleteResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Delete);
                if (deleteResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new DiscardItemResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                var result = await Deserialize<DiscardItemResponse>(deleteResponseMessage);
                return result;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new DiscardItemResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new DiscardItemResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new DiscardItemResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }


        private long _item;
        private bool _muling;
        public DiscardItemRequest(FUTAccount account, long item, bool isMuling, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _item = item;
            _muling = isMuling;
        }
    }
}
