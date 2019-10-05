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
    public class Sells : NancyModule
    {
        public Sells()
        {
            this.RequiresAuthentication();
            Get("/sells", args =>
            {
                var model = new BuysSellsModel();
                model.PriceString = "SellPrice";

                var page = int.Parse(Request.Query["page"].Value ?? "0");
                if (page < 0)
                {
                    page = 0;
                }
                var logsFrom = page * 15;
                var logsTo = 15;
                var futLogs = new List<FUTSell>();

                var logType = (string)Request.Query["type"].Value ?? "CMB";

                var assetID = int.Parse(Request.Query["assetid"].Value ?? "0");
                var revID = int.Parse(Request.Query["revid"].Value ?? "0");

                switch (logType)
                {
                    case "BIN":
                        model.Title = "Sells BIN";
                        model.SellsLogsBINActive = "active";
                        model.TreeviewBINActive = "active";
                        futLogs = FUTLogsDatabase.GetFUTSells(logsFrom, logsTo, FUTBuyBidType.BuyNow);
                        break;
                    case "BID":
                        model.Title = "Sells BID";
                        model.SellsLogsBIDActive = "active";
                        model.TreeviewBIDActive = "active";
                        futLogs = FUTLogsDatabase.GetFUTSells(logsFrom, logsTo, FUTBuyBidType.Bid);
                        break;
                    default:
                        logType = "CMB";
                        model.Title = "Sells Combined";
                        model.SellsLogsCMBActive = "active";
                        model.TreeviewCMBActive = "active";
                        futLogs = FUTLogsDatabase.GetFUTSells(logsFrom, logsTo);
                        break;
                }
                var checkNextPage = FUTLogsDatabase.CheckNextPageFUTSellsLogs(logsFrom + 15, logsTo);
                var checkPreviousPage = FUTLogsDatabase.CheckPreviousPageFUTSellsLogs(logsFrom, logsTo);
                model.Logs = new List<BuysSellsModel.SingleDataLog>();
                foreach (var futLog in futLogs)
                {
                    model.Logs.Add(new BuysSellsModel.SingleDataLog() { TimestampString = $"{Helper.TimestampToDateTime(futLog.Timestamp):d/M/yyyy HH:mm:ss}", ID = futLog.ID, TradeID = futLog.TradeID, RevisionID = futLog.RevisionID, ResourceID = futLog.AssetID, Price = futLog.SellPrice, ItemName = $"{futLog.ItemName} ({futLog.Rating})" });
                }

                if (checkPreviousPage)
                {
                    model.FooterPrevious = $"<a href=\"/sells?type={logType}&page={page - 1}\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>";
                }
                if (checkNextPage)
                {
                    model.FooterNext = $"<a href=\"/sells?type={logType}&page={page + 1}\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>";
                }
                return View["BuysSells", model];
            });
        }
    }
}
