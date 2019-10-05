using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class Index : NancyModule
    {
        public Index()
        {
            this.RequiresAuthentication();
            Get("/", args =>
            {
                var mod = new IndexModel();
                #region Coins & Accounts
                var coinsPerAccount = 0;
                var accounts = FUTAccountsDatabase.GetFUTAccounts();
                var coins = FUTLogsDatabase.GetFUTCoins();

                var totalCoins = accounts.Select(acc => coins.FirstOrDefault(x => x.EMail.ToLower() == acc.EMail.ToLower())).Where(coinsFromAcc => coinsFromAcc != null).Sum(coinsFromAcc => coinsFromAcc.Coins);
                if (totalCoins > 0 && accounts.Count > 0)
                {
                    coinsPerAccount = totalCoins / accounts.Count;
                }

                mod.TotalCoins = totalCoins;
                mod.AvgCoinsPerAccount = coinsPerAccount;
                mod.TotalAccounts = accounts.Count;


                var allTpItems = Fifa.Managers.BotManager.GetTradepileItems();
                var tpValue = (int)(allTpItems.Sum(x => x.buyNowPrice) * 0.95);

                mod.TotalOverallValue = tpValue + totalCoins;
                mod.TotalTradepileValue = tpValue;
                mod.TotalTradepileItems = allTpItems.Count;
                #endregion

                #region Logs
                mod.TotalBuys = FUTLogsDatabase.GetFUTBuysCount();
                mod.TotalSells = FUTLogsDatabase.GetFUTSellsCount();
                mod.TotalLogs = mod.TotalBuys + mod.TotalSells;
                #endregion

                if (!string.IsNullOrEmpty(HttpUtility.ParseQueryString(Request.Url.Query).Get("forbidden")))
                {
                    mod.DisplayError = true;
                    mod.ErrorMessage = "You do not have permissions to view this page!";
                }

                return View["Index", mod];
            });
        }
    }
}
