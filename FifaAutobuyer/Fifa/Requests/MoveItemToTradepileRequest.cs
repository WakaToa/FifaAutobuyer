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
    public class MoveItemToTradepileRequest : FUTRequestBase, IFUTRequest<MoveItemResponse>
    {
        public async Task<MoveItemResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);

                AddMethodOverrideHeader(HttpMethod.Put);
                var content = new StringContent("{\"itemData\":[{\"pile\":\"trade\", \"id\":\"" + _itemID + "\"}]}");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/item";
                uriString += "?_=" + Helper.CreateTimestamp();
                var searchResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Put);
                if (searchResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new MoveItemResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }

                
                var result = await Deserialize<MoveItemResponse>(searchResponseMessage);
                return result;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new MoveItemResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new MoveItemResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new MoveItemResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }


        private long _itemID;
        public MoveItemToTradepileRequest(FUTAccount account, long itemID, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _itemID = itemID;
        }
    }
}
