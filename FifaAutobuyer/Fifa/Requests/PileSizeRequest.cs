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
    public class PileSizeRequest : FUTRequestBase, IFUTRequest<PileSizeResponse>
    {
        public async Task<PileSizeResponse> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Get);
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/clientdata/pileSize";
                uriString += "?_=" + Helper.CreateTimestamp();
                var pileResponseMessage = await HttpClient.GetAsync(uriString).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Get);
                if (pileResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    var resp = new PileSizeResponse();
                    resp.Code = FUTErrorCode.CaptchaException;
                    return resp;
                }
                var result = await Deserialize<PileSizeResponse>(pileResponseMessage).ConfigureAwait(false);
                return result;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException.GetType() == typeof(SocketException))
                {
                    var resp = new PileSizeResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.ProxyException;
                    return resp;
                }
                else
                {
                    var resp = new PileSizeResponse();
                    resp.Message = httpEx.ToString();
                    resp.Code = FUTErrorCode.HttpRequestException;
                    return resp;
                }
            }
            catch (Exception e)
            {
                var resp = new PileSizeResponse();
                resp.Message = e.ToString();
                resp.Code = FUTErrorCode.RequestException;
                return resp;
            }
        }

        public PileSizeRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login) { }
    }
}
