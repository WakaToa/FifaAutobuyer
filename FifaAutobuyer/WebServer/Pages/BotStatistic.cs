using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class BotStatistic : NancyModule
    {
        public BotStatistic()
        {
            this.RequiresAuthentication();
            Get("/botstatistic", args =>
            {
                var model = new BotStatisticModel();

                model.TotalProfit = FUTLogsDatabase.GetFUTProfitLogsLast24Hours().Sum(x => x.Profit);
                model.BotStatistic = FUTLogsDatabase.GetFUTBotStatisticsLast24Hours();

                return View["BotStatistic", model];
            });
        }
    }
}
