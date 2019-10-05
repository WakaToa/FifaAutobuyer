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
    public class RelistAllRequest : FUTRequestBase, IFUTRequest<bool>
    {
        public async Task<bool> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Put);
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/auctionhouse/relist";
                uriString += "?_=" + Helper.CreateTimestamp();
                var creditsResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Put);
                if (creditsResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    return false;
                }
                var result = await creditsResponseMessage.Content.ReadAsStringAsync();
                var r = await Deserialize<CreditsResponse>(result);
                return !r.HasError;
            }
            catch (HttpRequestException httpEx)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public RelistAllRequest(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login) { }
    }
}
