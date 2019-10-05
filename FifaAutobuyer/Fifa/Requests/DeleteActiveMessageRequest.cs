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
    public class DeleteActiveMessageRequest : FUTRequestBase, IFUTRequest<bool>
    {
        public DeleteActiveMessageRequest(FUTAccount account,int id,  RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod loginMethod) : base(account, rpmManager, rpmManagerSearch, loginMethod)
        {
            _id = id;
        }

        private int _id;
        public async Task<bool> PerformRequestAsync()
        {
            try
            {
                await RequestPerMinuteManager.WaitForNextRequest().ConfigureAwait(false);
                AddMethodOverrideHeader(HttpMethod.Delete);
                var uriString = FUTAccount.FUTPlatform.Route + "/ut/game/fifa18/activeMessage/" + _id;
                uriString += "?_=" + Helper.CreateTimestamp();
                var content = new StringContent(" ");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


                var userResponseMessage = await HttpClient.PostAsync(uriString, content).ConfigureAwait(false);
                RemoveMethodOverrideHeader(HttpMethod.Delete);
                if (userResponseMessage.Headers.Contains("Proxy-Authorization"))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
