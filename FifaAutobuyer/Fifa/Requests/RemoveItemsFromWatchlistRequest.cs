using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using FifaAutobuyer.Fifa.Services;

namespace FifaAutobuyer.Fifa.Requests
{
    class RemoveItemsFromWatchlistRequest : FUTRequestBase, IFUTRequest<bool>
    {
        public async Task<bool> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);

                if (CurrentLoginMethod == LoginMethod.Web)
                {
                    var ids = String.Join("%2C", _tradeIDs);
                    var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/watchlist?tradeId=" + ids;
                    uriString += "&_=" + Helper.CreateTimestamp();
                    AddMethodOverrideHeader(HttpMethod.Delete);
                    var content = new StringContent(" ");
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    var removeResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                    RemoveMethodOverrideHeader(HttpMethod.Delete);
                    var rr = await removeResponseMessage.Content.ReadAsStringAsync();
                    return removeResponseMessage.StatusCode == HttpStatusCode.OK;
                }
                else
                {
                    var ids = String.Join(",", _tradeIDs);
                    var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/watchlist?tradeId=" + ids;
                    uriString += "&_=" + Helper.CreateTimestamp();
                    var removeResponseMessage = await HttpClient.DeleteAsync(uriString).ConfigureAwait(false);
                    var rr = await removeResponseMessage.Content.ReadAsStringAsync();
                    return removeResponseMessage.StatusCode == HttpStatusCode.OK;
                }
               
            }
#pragma warning disable CS0168
            catch (Exception e)
            {
                return false;
            }
#pragma warning restore CS0168

        }


        private List<string> _tradeIDs;
        public RemoveItemsFromWatchlistRequest(FUTAccount account, List<string> tradeIDs, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _tradeIDs = tradeIDs;
        }
    }
}
