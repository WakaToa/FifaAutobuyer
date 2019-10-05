using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Newtonsoft.Json;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.Database.Settings;

namespace FifaAutobuyer.Fifa.Requests
{
    public class BuyTradeRequest : FUTRequestBase, IFUTRequest<BuyItemResponse>
    {
        public async Task<BuyItemResponse> PerformRequestAsync()
        {
            try
            {
                var uriString = string.Format(FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/trade/{0}/bid", _tradeID);
                uriString += "?_=" + Helper.CreateTimestamp();
                AddMethodOverrideHeader(HttpMethod.Put);
                var content = new StringContent("{\"bid\":" + _price + "}");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var searchResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Put);
                if(searchResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new BuyItemResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                var result = await Deserialize<BuyItemResponse>(searchResponseMessage);
                return result;
            }
            catch(HttpRequestException httpEx)
            {
                if(httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new BuyItemResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new BuyItemResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new BuyItemResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }
        }


        private long _price;
        private long _tradeID;
        public BuyTradeRequest(FUTAccount account, long tradeID, long price,  RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _price = price;
            _tradeID = tradeID;
        }
    }
}
