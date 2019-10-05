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
    public class Buys : NancyModule
    {
        public Buys()
        {
            this.RequiresAuthentication();
            Get("/buys", args =>
            {
                var model = new BuysSellsModel();
                model.PriceString = "BuyPrice";

                var page = int.Parse(Request.Query["page"].Value ?? "0");
                if (page < 0)
                {
                    page = 0;
                }
                var logsFrom = page * 15;
                var logsTo = 15;
                var futLogs = new List<FUTBuy>();

                var logType = (string)Request.Query["type"].Value ?? "CMB";
                switch (logType)
                {
                    case "BIN":
                        model.Title = "Buys BIN";
                        model.LogType = "BIN";
                        model.BuysLogsBINActive = "active";
                        model.TreeviewBINActive = "active";
                        futLogs = FUTLogsDatabase.GetFUTBuys(logsFrom, logsTo, FUTBuyBidType.BuyNow);
                        break;
                    case "BID":
                        model.Title = "Buys BID";
                        model.LogType = "BID";
                        model.BuysLogsBIDActive = "active";
                        model.TreeviewBIDActive = "active";
                        futLogs = FUTLogsDatabase.GetFUTBuys(logsFrom, logsTo, FUTBuyBidType.Bid);
                        break;
                    default:
                        logType = "CMB";
                        model.LogType = "CMB";
                        model.Title = "Buys Combined";
                        model.BuysLogsCMBActive = "active";
                        model.TreeviewCMBActive = "active";
                        futLogs = FUTLogsDatabase.GetFUTBuys(logsFrom, logsTo);
                        break;
                }
                var checkNextPage = FUTLogsDatabase.CheckNextPageFUTBuysLogs(logsFrom + 15, logsTo);
                var checkPreviousPage = FUTLogsDatabase.CheckPreviousPageFUTBuysLogs(logsFrom, logsTo);
                model.Logs = new List<BuysSellsModel.SingleDataLog>();
                foreach (var futLog in futLogs)
                {
                    model.Logs.Add(new BuysSellsModel.SingleDataLog() {TimestampString = $"{Helper.TimestampToDateTime(futLog.Timestamp):d/M/yyyy HH:mm:ss}", ID =  futLog.ID, TradeID = futLog.TradeID, RevisionID = futLog.RevisionID, ResourceID = futLog.AssetID, Price = futLog.BuyNowPrice, ItemName = $"{futLog.ItemName} ({futLog.Rating})"});
                }

                if (checkPreviousPage)
                {
                    model.FooterPrevious = $"<a href=\"/buys?type={logType}&page={page - 1}\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>";
                }
                if (checkNextPage)
                {
                    model.FooterNext = $"<a href=\"/buys?type={logType}&page={page + 1}\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>";
                }
                return View["BuysSells", model];
            });
        }
    }
}
