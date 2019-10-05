using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Responses;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class GeneralSettings : NancyModule
    {
        public GeneralSettings()
        {
            this.RequiresAuthentication();
            Get("/generalsettings", args =>
            {
                var model = new GeneralSettingsModel();
                return View["GeneralSettings", model];
            });

            Post("/generalsettings", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                FUTSettings.Instance.RoundsPerMinuteMin = int.Parse(parameters["rpmMin"]);
                FUTSettings.Instance.RoundsPerMinuteMax = int.Parse(parameters["rpmMax"]);
                FUTSettings.Instance.RoundsPerMinuteMinSearch = int.Parse(parameters["rpmMinSearch"]);
                FUTSettings.Instance.RoundsPerMinuteMaxSearch = int.Parse(parameters["rpmMaxSearch"]);
                FUTSettings.Instance.PauseBetweenRelogs = int.Parse(parameters["pauseRelogs"]);
                FUTSettings.Instance.TradepileCheck = int.Parse(parameters["tradepileCheckTimes"]);
                FUTSettings.Instance.Counter = int.Parse(parameters["counterBase"]);
                FUTSettings.Instance.PriceCorrectionPercentage = int.Parse(parameters["priceCorrectionPercentage"]);
                FUTSettings.Instance.PriceCheckTimes = int.Parse(parameters["pricecheckTime"]);
                FUTSettings.Instance.MinimumPlayersForPriceCheck = int.Parse(parameters["minimumPlayersForPriceCheck"]);
                FUTSettings.Instance.EnableBuy = parameters["enableBuy"] != null;
                FUTSettings.Instance.EnableSell = parameters["enableSell"] != null;
                FUTSettings.Instance.RelistWithOldPrice = parameters["relistWithOldPrice"] != null;
                FUTSettings.Instance.DiscardEverything = parameters["discardEverything"] != null;
                FUTSettings.Instance.LoginMethod = parameters["currentLoginMethod"] == "web"
                    ? LoginMethod.Web
                    : parameters["currentLoginMethod"] == "ios"
                        ? LoginMethod.IOS
                        : LoginMethod.Android;
                FUTSettings.Instance.UseBidSwitch = parameters["useBidSwitch"] != null;
                FUTSettings.Instance.UseRandomRequests = parameters["useRandomRequests"] != null;
                FUTSettings.Instance.OneParallelLogin = parameters["oneParallelLogin"] != null;
                FUTSettings.Instance.WatchlistCheck = int.Parse(parameters["watchlistCheckTimes"]);
                FUTSettings.Instance.ExpiredTimer = int.Parse(parameters["expiredTimer"]);
                FUTSettings.Instance.WaitAfterBuy = int.Parse(parameters["waitAfterBuy"]);
                FUTSettings.Instance.MaxCardsPerDay = int.Parse(parameters["maxCardsPerDay"]);
                FUTSettings.Instance.UseLastPriceChecks = int.Parse(parameters["useLastPriceChecks"]);
                FUTSettings.Instance.SaveChanges();

                return Response.AsRedirect("/generalsettings");
            });
        }
    }
}
