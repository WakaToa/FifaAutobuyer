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
    public class ExceptionLogs : NancyModule
    {
        public ExceptionLogs()
        {
            this.RequiresAuthentication();
            Get("/exceptionlogs", args =>
            {
                var model = new DataLogsModel();
                model.ExceptionLogsActive = "active";
                model.Title = "Exception Logs";

                var page = int.Parse(Request.Query["page"].Value ?? "0");
                if (page < 0)
                {
                    page = 0;
                }
                var logsFrom = page * 15;
                var logsTo = 15;
                var futLogs = FUTLogsDatabase.GetFUTExceptionLogs(logsFrom, logsTo);
                var checkNextPage = FUTLogsDatabase.CheckNextPageFUTExceptionLogs(logsFrom + 15, logsTo);
                var checkPreviousPage = FUTLogsDatabase.CheckPreviousPageFUTExceptionLogs(logsFrom, logsTo);
                model.Logs = new List<DataLogsModel.SingleDataLog>();
                foreach (var futLog in futLogs)
                {
                    model.Logs.Add(new DataLogsModel.SingleDataLog() {ID =  futLog.ID, Account = futLog.EMail, Data = futLog.Data, Timestamp = $"{Helper.TimestampToDateTime(futLog.Timestamp):d/M/yyyy HH:mm:ss}" });
                }

                if (checkPreviousPage)
                {
                    model.FooterPrevious = $"<a href=\"/exceptionlogs?page={page - 1}\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>";
                }
                if (checkNextPage)
                {
                    model.FooterNext = $"<a href=\"/exceptionlogs?page={page + 1}\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>";
                }
                return View["DataLogs", model];
            });
        }
    }
}
