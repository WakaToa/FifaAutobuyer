using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class AccountStatistic : NancyModule
    {
        public AccountStatistic()
        {
            this.RequiresAuthentication();
            Get("/accountstatistic", args =>
            {
                var model = new AccountStatisticModel();
                //        public long ProfitLast24Hours =>;
                var clients = new List<Pair<FUTClient, long>>();

                var profits = FUTLogsDatabase.GetFUTProfitLogsLast24Hours();

                foreach (var futClient in Fifa.Managers.BotManager.GetFutClients())
                {
                    var profit = profits.Where(x => x.Account.ToLower() == futClient.FUTAccount.EMail.ToLower())
                        .Sum(x => x.Profit);
                    clients.Add(new Pair<FUTClient, long>(futClient, profit));
                }
                model.FUTClients = clients;
                return View["AccountStatistic", model];
            });

            Post("/startaccount", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var account = parameters["account"];

                Fifa.Managers.BotManager.StartBot(account);

                return Response.AsText("true");
            });

            Post("/stopaccount", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var account = parameters["account"];

                Fifa.Managers.BotManager.StopBot(account);

                return Response.AsText("true");
            });
        }
    }
}
