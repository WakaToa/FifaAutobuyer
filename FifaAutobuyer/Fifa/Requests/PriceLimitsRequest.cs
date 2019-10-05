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
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.Database.Settings;

namespace FifaAutobuyer.Fifa.Requests
{
    public class PriceLimitsRequest : FUTRequestBase, IFUTRequest<PriceLimitsResponse>
    {
        public async Task<PriceLimitsResponse> PerformRequestAsync()
        {
            try
            {
                
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);

             
                var uriString = string.Format(FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/marketdata/pricelimits?defId={0}", _assetID);
                uriString += "&_=" + Helper.CreateTimestamp();
                AddMethodOverrideHeader(HttpMethod.Get);
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var limitsResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
                if (limitsResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new PriceLimitsResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                var contentString = await limitsResponseMessage.Content.ReadAsStringAsync();
                if (contentString.Length > 2)
                {
                    if (contentString.StartsWith("["))
                    {
                        contentString = contentString.Remove(0, 1);
                    }
                    if (contentString.EndsWith("]"))
                    {
                        contentString = contentString.Remove(contentString.Length - 1, 1);
                    }

                }
                var result = await Deserialize<PriceLimitsResponse>(contentString);
                return result;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new PriceLimitsResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new PriceLimitsResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new PriceLimitsResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }

        }

        private long _assetID;
        public PriceLimitsRequest(FUTAccount account, long assetID, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _assetID = assetID;
        }
    }
}
