using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using FifaAutobuyer.Fifa.Services;

namespace FifaAutobuyer.Fifa.Requests
{
    public class TransferMarketRequest : FUTRequestBase, IFUTRequest<TransferMarketResponse>
    {
        public async Task<TransferMarketResponse> PerformRequestAsync()
        {
            try
            {
                if (!_muling)
                {
                    await RequestPerMinuteManagerSearch.WaitForNextRequest().ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(2500);
                }
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/transfermarket";

                uriString += _item.BuildUriString();
                
                if (_page <= 0)
                {
                    _page = 1;
                }
                if (_page > 1)
                {
                    uriString += "&num=16";
                    uriString += "&start=" + (_page - 1) * 15;
                }
                else
                {
                    uriString += "&num=16";
                    uriString += "&start=0";
                }
                uriString += "&_=" + Helper.CreateTimestamp();
                AddMethodOverrideHeader(HttpMethod.Get);
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var searchResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
                if (searchResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new TransferMarketResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                var result = await Deserialize<TransferMarketResponse>(searchResponseMessage);
                return result;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new TransferMarketResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new TransferMarketResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new TransferMarketResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }

        public void SetPage(int page)
        {
            _page = page;
        }
        private int _page = 1;
        private FUTSearchParameter _item;
        private bool _muling;
        public TransferMarketRequest (FUTAccount account, FUTSearchParameter item, bool isMuling, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _item = item;
            _muling = isMuling;
        }
    }
}
