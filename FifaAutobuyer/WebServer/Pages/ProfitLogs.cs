using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class ProfitLogs : NancyModule
    {
        public ProfitLogs()
        {
            this.RequiresAuthentication();
            Get("/profitlogs", args =>
            {
                var model = new ProfitLogsModel();
                var page = int.Parse(Request.Query["page"].Value ?? "0");
                if (page < 0)
                {
                    page = 0;
                }
                var logsFrom = page * 15;
                var logsTo = 15;
                var futLogs = new List<FUTItemProfit>();

                var logType = (string)Request.Query["type"].Value ?? "CMB";

                var assetID = int.Parse(Request.Query["assetid"].Value ?? "0");
                var revID = int.Parse(Request.Query["revid"].Value ?? "0");

                switch (logType)
                {
                    case "BIN":
                        model.Title = "Profit Logs BIN";
                        model.ProfitLogsBINActive = "active";
                        model.LogType = "BIN";
                        model.TreeviewBINActive = "active";
                        futLogs = assetID != 0 ? FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo, assetID, revID, FUTBuyBidType.BuyNow) : FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo , FUTBuyBidType.BuyNow);
                        break;
                    case "BID":
                        model.Title = "Profit Logs BID";
                        model.ProfitLogsBIDActive = "active";
                        model.LogType = "BID";
                        model.TreeviewBIDActive = "active";
                        futLogs = assetID != 0 ? FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo, assetID, revID, FUTBuyBidType.Bid) : FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo, FUTBuyBidType.Bid);
                        break;
                    default:
                        model.Title = "Profit Logs CMB";
                        logType = "CMB";
                        model.ProfitLogsCMBActive = "active";
                        model.LogType = "CMB";
                        model.TreeviewCMBActive = "active";
                        futLogs = assetID != 0 ? FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo, assetID, revID) : FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo);
                        break;
                }
                var checkNextPage = FUTLogsDatabase.CheckNextPageFUTProfitLogs(logsFrom + 15, logsTo);
                var checkPreviousPage = FUTLogsDatabase.CheckPreviousPageFUTProfitLogs(logsFrom, logsTo);
                model.Logs = new List<ProfitLogsModel.SingleDataLog>();
                foreach (var futLog in futLogs)
                {
                    model.Logs.Add(new ProfitLogsModel.SingleDataLog() {ResourceID = futLog.AssetID, RevisionID = futLog.RevisionID, ID = futLog.ID, ItemName = $"{futLog.ItemName} ({futLog.Rating} / {futLog.RevisionID} / {futLog.Position} / {futLog.ChemistryStyle})", SellPrice = futLog.SellPrice, BuyPrice = futLog.BuyPrice, Profit = futLog.Profit, BoughtOn = $"{Helper.TimestampToDateTime(futLog.BuyTimestamp):d/M/yyyy HH:mm:ss}", SoldOn = $"{Helper.TimestampToDateTime(futLog.SellTimestamp):d/M/yyyy HH:mm:ss}", TimeOnTradepile = Helper.TimestampToDateTime(futLog.SellTimestamp).Subtract(Helper.TimestampToDateTime(futLog.BuyTimestamp)).ToReadableString()});
                }

                if (checkPreviousPage)
                {
                    model.FooterPrevious = $"<a href=\"/profitlogs?type={logType}&page={page - 1}\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>";
                }
                if (checkNextPage)
                {
                    model.FooterNext = $"<a href=\"/profitlogs?type={logType}&page={page + 1}\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>";
                }
                return View["ProfitLogs", model];
            });
        }
    }
}
