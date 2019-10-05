using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class BotLogs  :NancyModule
    {
        public BotLogs()
        {
            this.RequiresAuthentication();
            Get("/botlogs", args =>
            {
                var model = new DataLogsModel();
                model.BotLogsActive = "active";
                model.Title = "Bot Logs";

                var page = int.Parse(Request.Query["page"].Value ?? "0");
                if (page < 0)
                {
                    page = 0;
                }
                var email = Request.Query["email"].Value ?? "";


                var logsFrom = page * 15;
                var logsTo = 15;
                var futLogs = email != "" ? FUTLogsDatabase.GetFUTBotLogsByEMail(email, logsFrom, logsTo) : FUTLogsDatabase.GetFUTBotLogs(logsFrom, logsTo);

                var checkNextPage = FUTLogsDatabase.CheckNextPageFUTBotLogs(logsFrom + 15, logsTo);
                var checkPreviousPage = FUTLogsDatabase.CheckPreviousPageFUTBotLogs(logsFrom, logsTo);

                model.Logs = new List<DataLogsModel.SingleDataLog>();
                foreach (var futLog in futLogs)
                {
                    model.Logs.Add(new DataLogsModel.SingleDataLog() { ID = futLog.ID, Account = futLog.EMail, Data = futLog.Data, Timestamp = $"{Helper.TimestampToDateTime(futLog.Timestamp):d/M/yyyy HH:mm:ss}" });
                }

                var nextURL = "/botlogs?page=" + (page + 1);
                var previousURL = "/botlogs?page=" + (page - 1);
                if (email != "")
                {
                    nextURL += "&email=" + email;
                    previousURL += "&email=" + email;
                }

                if (checkPreviousPage)
                {
                    model.FooterPrevious = $"<a href=\"{previousURL}\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>";
                }
                if (checkNextPage)
                {
                    model.FooterNext = $"<a href=\"{nextURL}\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>";
                }

                return View["DataLogs", model];
            });
        }
    }
}
