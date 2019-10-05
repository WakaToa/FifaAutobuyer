using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.Database.Settings;

namespace FifaAutobuyer.Fifa.Requests
{

    public class RemoveItemFromTradepileRequest : FUTRequestBase, IFUTRequest<bool>
    {
        public async Task<bool> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                
                var uriString = string.Format(FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/trade/{0}", _tradeID);
                uriString += "?_=" + Helper.CreateTimestamp();
                AddMethodOverrideHeader(HttpMethod.Delete);
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var removeResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Delete);
                return removeResponseMessage.StatusCode == HttpStatusCode.OK;
            }
#pragma warning disable CS0168
            catch (Exception e)
            {
                return false;
            }
#pragma warning restore CS0168

        }


        private long _tradeID;
        public RemoveItemFromTradepileRequest(FUTAccount account, long tradeID, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod login) : base(account, rpmManager, rpmManagerSearch, login)
        {
            _tradeID = tradeID;
        }
    }
}
