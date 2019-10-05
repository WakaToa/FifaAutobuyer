using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Database.Web;
using FifaAutobuyer.Fifa;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FifaAutobuyer.Web
{
    public class HttpServer
    {
        private HttpListener _httpListener;
        private int _port = 8080;

        public void Start()
        {
            _port = int.Parse(ConfigurationManager.AppSettings["WebAppPort"]);
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(String.Format("http://*:{0}{1}", _port, "/"));
            _httpListener.Start();

            _httpListener.BeginGetContext(new AsyncCallback(HttpContextReceivedCallback), null);
        }

        private void HttpContextReceivedCallback(IAsyncResult asyncResult)
        {
            HttpListenerContext context = null;
            try
            {
                context = _httpListener.EndGetContext(asyncResult);
                _httpListener.BeginGetContext(new AsyncCallback(HttpContextReceivedCallback), null);

                if(context.Request.Url.AbsolutePath.EndsWith(".html"))
                {
                    Write404NotFound(context);
                    return;
                }
                switch (context.Request.Url.AbsolutePath)
                {
                    case "/":
                        HandleIndexPage(context);
                        break;
                    case "/index":
                        HandleIndexPage(context);
                        break;
                    case "/login":
                        HandleLoginPage(context);
                        break;
                    case "/logout":
                        HandleLogoutPage(context);
                        break;
                    case "/botmanager":
                        HandleBotManagerPage(context);
                        break;
                    case "/profitlogs":
                        HandleProfitLogsPage(context);
                        break;
                    case "/botlogs":
                        HandleBotLogsPage(context);
                        break;
                    case "/exceptionlogs":
                        HandleExceptionLogsPage(context);
                        break;
                    case "/buys":
                        HandleBuysPage(context);
                        break;
                    case "/sells":
                        HandleSellsPage(context);
                        break;
                    case "/generalsettings":
                        HandleGeneralSettingsPage(context);
                        break;
                    case "/managelist":
                        HandleManageListPage(context);
                        break;
                    case "/searchforitem":
                        HandleSearchForItemRequest(context);
                        break;
                    case "/additem":
                        HandleAddItemRequest(context);
                        break;
                    case "/addaccount":
                        HandleAddAccountPage(context);
                        break;
                    case "/editaccount":
                        HandleEditAccountPage(context);
                        break;
                    case "/getaccountinformation":
                        HandleGetAccountInformationRequest(context);
                        break;
                    case "/resetpricecheck":
                        HandleResetPriceCheckRequest(context);
                        break;
                    case "/itemstatistic":
                        HandleItemStatisticPage(context);
                        break;
                    case "/accountstatistic":
                        HandleAccountStatisticPage(context);
                        break;
                    case "/startaccount":
                        StartSingleAccountRequest(context);
                        break;
                    case "/stopaccount":
                        StopSingleAccountRequest(context);
                        break;
                    case "/notificationcenter":
                        HandleNotificationCenterPage(context);
                        break;
                    case "/acknowledgenotification":
                        AcknowledgeNotificationRequest(context);
                        break;
                    case "/acknowledgenotificationarray":
                        AcknowledgeNotificationArrayRequest(context);
                        break;
                    case "/mule":
                        HandleMulePage(context);
                        break;
                    case "/getmulestatus":
                        GetMuleStatusRequest(context);
                        break;
                    case "/removeitemfromlist":
                        HandleRemoveItemFromListRequest(context);
                        break;
                    case "/saveitem":
                        HandleSaveItemRequest(context);
                        break;
                    case "/update.json":
                        HandleUpdateItemsJsonRequest(context);
                        break;
                    case "/resetpricechecks":
                        HandleResetPricechecksRequest(context);
                        break;
                    case "/botstatistic":
                        HandleBotStatisticPage(context);
                        break;
                    case "/mulesession":
                        HandleMuleSessionPage(context);
                        break;
                    case "/removemulesession":
                        RemoveMuleSession(context);
                        break;
                    default:
                        HandleSpecialRequest(context);
                        break;
                }
            }
            catch(Exception e)
            {
                if(context != null)
                {
                    try
                    {
                        Write500ServerError(context, e.ToString());
                    }
                    catch { }
                    
                }
                
            }
        }

        private string GetPlatformFromConfig()
        {
            var id = int.Parse(ConfigurationManager.AppSettings["Platform"]);
            if(id == 0)
            {
                return "Playstation 4";
            }
            else if (id == 1)
            {
                return "Playstation 3";
            }
            else if(id == 2)
            {
                return "Xbox 360";
            }
            else if(id == 3)
            {
                return "Xbox One";
            }
            else
            {
                return "Unknown Platform";
            }
        }

        #region Login/Logout/Index
        private void HandleLoginPage(HttpListenerContext context)
        {
            if(VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }
            if(context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                var loginPage = File.ReadAllText("Autobuyer.web/login.html");
                Write200Success(context, loginPage, "text/html");
            }
            if(context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var username = parameters["username"];
                var password = parameters["password"];

                if(!WebSessionVerifier.CheckUsernamePassword(username, password))
                {
                    var loginPage = File.ReadAllText("Autobuyer.web/login.html");
                    loginPage = loginPage.Replace("box-danger\" style=\"display:none\"", "box-danger\"");
                    Write200Success(context, loginPage);
                    return;
                }

                var sessionID = WebSessionsDatabase.GenerateSessionID();
                var salt = Helper.RandomString(12);
                WebSessionsDatabase.SetWebSession(username, sessionID, salt);
                
                context.Response.Cookies.Add(new Cookie("X-AB-SID_" + salt, sessionID));
                context.Response.Cookies.Add(new Cookie("X-AB-USER_" + salt, username));

                var redirect = HttpUtility.ParseQueryString(context.Request.Url.Query);
                var redirectUri = redirect["redirectUrl"];
                if(!String.IsNullOrEmpty(redirectUri))
                {
                    context.Response.Redirect(redirectUri);
                    context.Response.Close();
                    return;
                }
                else
                {
                    context.Response.Redirect("/");
                    context.Response.Close();
                    return;
                }
            }
        }

        private void HandleLogoutPage(HttpListenerContext context)
        {
            var allSalts = WebSessionsDatabase.GetWebSessions().Select(x => x.Salt).ToList();
            Cookie sid = null;
            Cookie user = null;
            foreach(var salt in allSalts)
            {
                if(context.Request.Cookies["X-AB-SID_" + salt] != null)
                {
                    sid = context.Request.Cookies["X-AB-SID_" + salt];
                    user = context.Request.Cookies["X-AB-USER_" + salt];
                }
            }
            if (sid != null && user != null)
            {
                WebSessionsDatabase.SetWebSession(user.Value, "", "");
                sid.Expired = true;
                sid.Expires = DateTime.Now.AddDays(-1);
                user.Expired = true;
                user.Expires = DateTime.Now.AddDays(-1);
                context.Response.SetCookie(sid);
                context.Response.SetCookie(user);
            }
            context.Response.Redirect("/login");
            context.Response.Close();
            return;
        }

        private void HandleIndexPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            var data = File.ReadAllText("Autobuyer.web/index.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }

            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());

            #region Load Replacing
            //var ethernetLoad = Performance.PerformanceCounterEx.GetEthernetLoad();
            //data = data.Replace("{ETHERNETLOADPERCENT}", ethernetLoad.ToString());
            //if (ethernetLoad > 80)
            //{
            //    data = data.Replace("class=\"info-box bg-green\" id=\"ethernetLoad", "class=\"info-box bg-red\" id=\"ethernetLoad");
            //}
            //if (ethernetLoad > 50 && ethernetLoad < 80)
            //{
            //    data = data.Replace("class=\"info-box bg-green\" id=\"ethernetLoad", "class=\"info-box bg-yellow\" id=\"ethernetLoad");
            //}
            //var ramLoad = Performance.PerformanceCounterEx.GetRAMLoad();
            //data = data.Replace("{RAMLOADPERCENT}", ramLoad.ToString());
            //if (ramLoad > 80)
            //{
            //    data = data.Replace("class=\"info-box bg-green\" id=\"ramLoad", "class=\"info-box bg-red\" id=\"ramLoad");
            //}
            //if (ramLoad > 50 && ramLoad < 80)
            //{
            //    data = data.Replace("class=\"info-box bg-green\" id=\"ramLoad", "class=\"info-box bg-yellow\" id=\"ramLoad");
            //}
            //var cpuLoad = Performance.PerformanceCounterEx.GetCPULoad();
            //data = data.Replace("{CPULOADPERCENT}", cpuLoad.ToString());
            //if (cpuLoad > 80)
            //{
            //    data = data.Replace("class=\"info-box bg-green\" id=\"cpuLoad", "class=\"info-box bg-red\" id=\"cpuLoad");
            //}
            //if (cpuLoad > 50 && cpuLoad < 80)
            //{
            //    data = data.Replace("class=\"info-box bg-green\" id=\"cpuLoad", "class=\"info-box bg-yellow\" id=\"cpuLoad");
            //}

            data = data.Replace("{ETHERNETLOADPERCENT}", "0");
            data = data.Replace("{RAMLOADPERCENT}", "0");
            data = data.Replace("{CPULOADPERCENT}", "0");
            #endregion

            #region Coins and Accounts
            var totalCoins = 0;
            var coinsPerAccount = 0;
            var accounts = FUTAccountsDatabase.GetFUTAccounts();
            var coins = FUTLogsDatabase.GetFUTCoins();
            
            foreach(var acc in accounts)
            {
                var coinsFromAcc = coins.Where(x => x.EMail.ToLower() == acc.EMail.ToLower()).FirstOrDefault();
                if(coinsFromAcc != null)
                {
                    totalCoins += coinsFromAcc.Coins;
                }
            }
            if(totalCoins > 0 && accounts.Count > 0)
            {
                coinsPerAccount = totalCoins / accounts.Count;
            }
            data = data.Replace("{TOTALCOINS}", totalCoins.ToString());
            data = data.Replace("{TOTALACCOUNTS}", accounts.Count.ToString());
            data = data.Replace("{COINSPERACCOUNT}", coinsPerAccount.ToString());


           

            #endregion

            #region Logs
            var totalLogs = 0;//FUTLogsDatabase.GetFUTLogs().Count;
            var totalBuys = FUTLogsDatabase.GetFUTBuysCount();
            var totalSells = FUTLogsDatabase.GetFUTSellsCount();
            data = data.Replace("{TOTALLOGS}", (totalLogs + totalBuys + totalSells).ToString());
            data = data.Replace("{TOTALBUYS}", totalBuys.ToString());
            data = data.Replace("{TOTALSELLS}", totalSells.ToString());
            #endregion

            var allTPItems = BotManager.GetTradepileItems();
            var tpValue = (int)(allTPItems.Sum(x => x.buyNowPrice) * 0.95);


            data = data.Replace("{TOTALTRADEPILEVALUE}", tpValue.ToString());
            data = data.Replace("{TOTALOVERALLVALUE}", (tpValue + totalCoins).ToString());
            data = data.Replace("{TOTALTRADEPILEITEMS}", allTPItems.Count.ToString().ToString());


            var errorCode = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("access");
            if (!string.IsNullOrEmpty(errorCode))
            {
                if (errorCode == "no")
                {
                    data = data.Replace("id=\"errorBox\" style=\"display:none;\"", "id=\"errorBox\"");
                    data = data.Replace("{ERRORMESSAGE}", "You do not have permissions to view this page!");
                }
            }

            Write200Success(context, data, "text/html");
        }
        #endregion

        #region Logs
        private int _logsPerPage = 15;

        private void HandleProfitLogsPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/profitlogs.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());


            var page = 0;
            if (context.Request.QueryString["page"] != null)
            {
                page = int.Parse(context.Request.QueryString["page"]);
                if (page < 0)
                {
                    page = 0;
                }
            }

            var assetID = -1;
            if (context.Request.QueryString["assetid"] != null)
            {
                assetID = int.Parse(context.Request.QueryString["assetid"]);
            }
            var revID = -1;
            if (context.Request.QueryString["revid"] != null)
            {
                revID = int.Parse(context.Request.QueryString["revid"]);
            }

            FUTBuyBidType type = FUTBuyBidType.Combined;
            var typeString = "CMB";
            if (context.Request.QueryString["type"] != null)
            {
                if (context.Request.QueryString["type"].ToUpper() == "BIN")
                {
                    type = FUTBuyBidType.BuyNow;
                    data = data.Replace("{BINProfitLogsActive}", "active");
                    data = data.Replace("{BIDProfitLogsActive}", "");
                    data = data.Replace("{CMBProfitLogsActive}", "");
                }
                else if (context.Request.QueryString["type"].ToUpper() == "BID")
                {
                    type = FUTBuyBidType.Bid;
                    data = data.Replace("{BINProfitLogsActive}", "");
                    data = data.Replace("{BIDProfitLogsActive}", "active");
                    data = data.Replace("{CMBProfitLogsActive}", "");
                }
                else if(context.Request.QueryString["type"].ToUpper() == "CMB")
                {
                    type = FUTBuyBidType.Combined;
                    data = data.Replace("{BINProfitLogsActive}", "");
                    data = data.Replace("{BIDProfitLogsActive}", "");
                    data = data.Replace("{CMBProfitLogsActive}", "active");
                }
                else
                {
                    type = FUTBuyBidType.Combined;
                    data = data.Replace("{BINProfitLogsActive}", "");
                    data = data.Replace("{BIDProfitLogsActive}", "");
                    data = data.Replace("{CMBProfitLogsActive}", "active");
                }
                typeString = context.Request.QueryString["type"].ToUpper();
            }

            var stringBuilderProfits = new StringBuilder();
            var logsFrom = page * _logsPerPage;
            var logsTo = _logsPerPage;
            var futProfits = new List<FUTItemProfit>();
            if(assetID >= 0 && revID >= 0)
            {
                if (type == FUTBuyBidType.Combined)
                {
                    futProfits = FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo, assetID, revID);
                }
                else
                {
                    futProfits = FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo, assetID, revID, type);
                }
                
            }
            else
            {
                if (type == FUTBuyBidType.Combined)
                {
                    futProfits = FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo);
                }
                else
                {
                    futProfits = FUTLogsDatabase.GetFUTProfitLogs(logsFrom, logsTo, type);
                }
            }
                
            foreach (var log in futProfits)
            {
                stringBuilderProfits.AppendLine("<tr>");
                stringBuilderProfits.AppendLine("<td>" + log.ID + "</td>");
                SimpleSearchItemModel itemData = FUTItemManager.GetItemByAssetRevisionID(log.AssetID, log.RevisionID);
                var link = "<a target=\"_blank\" href=\"/profitlogs?type=" + typeString + "&assetid=" + log.AssetID + "&revid=" + log.RevisionID + "\">";
                if (itemData != null)
                {
                    stringBuilderProfits.AppendLine("<td>" + link + itemData.GetName() + " (" + itemData.r + ")</a></td>");
                }
                else
                {
                    stringBuilderProfits.AppendLine("<td>" + link + log.AssetID + "/" + log.RevisionID + "</a></td>");
                }
                stringBuilderProfits.AppendLine("<td>" + log.BuyPrice + "</td>");
                stringBuilderProfits.AppendLine("<td>" + log.SellPrice + "</td>");
                stringBuilderProfits.AppendLine("<td>" + log.Profit + "</td>");
                var dateTimeBuy = Helper.TimestampToDateTime(log.BuyTimestamp);
                var printDatebuy = dateTimeBuy.ToShortDateString() + " " + dateTimeBuy.ToLongTimeString();
                var dateTimeSell = Helper.TimestampToDateTime(log.SellTimestamp);
                var printDateSell = dateTimeSell.ToShortDateString() + " " + dateTimeSell.ToLongTimeString();
                stringBuilderProfits.AppendLine("<td>" + printDatebuy + "</td>");
                stringBuilderProfits.AppendLine("<td>" + printDateSell + "</td>");
                var difference = dateTimeSell.Subtract(dateTimeBuy);
                stringBuilderProfits.AppendLine("<td>" + difference.ToReadableString() + "</td>");
                stringBuilderProfits.AppendLine("</tr>");
            }

            data = data.Replace("{LOGDATA}", stringBuilderProfits.ToString());

            var nextURL = "/profitlogs?type=" + typeString + "&page=" + (page + 1);
            var previousURL = "/profitlogs?type=" + typeString + "&page=" + (page - 1);
            if (assetID > 0 && revID >= 0)
            {
                nextURL += "&assetid=" + assetID + "&revid=" + revID;
                previousURL += "&assetid=" + assetID + "&revid=" + revID;
            }

            if (page == 0)
            {
                data = data.Replace("<a href=\"/profitlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "");
                if (futProfits.Count < _logsPerPage)
                {
                    data = data.Replace("<a href=\"/profitlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
                }
                data = data.Replace("<a href=\"/profitlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"" + nextURL + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");


            }
            else if (futProfits.Count < _logsPerPage)
            {
                data = data.Replace("<a href=\"/profitlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"" + previousURL + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/profitlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
            }
            else
            {
                data = data.Replace("<a href=\"/profitlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"" + previousURL + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/profitlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"" + nextURL + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");
            }



            Write200Success(context, data, "text/html");
        }

        private void HandleBuysPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/buys.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
            var page = 0;
            if (context.Request.QueryString["page"] != null)
            {
                page = int.Parse(context.Request.QueryString["page"]);
                if (page < 0)
                {
                    page = 0;
                }
            }

            FUTBuyBidType type = FUTBuyBidType.Combined;
            var typeString = "CMB";
            if (context.Request.QueryString["type"] != null)
            {
                switch (context.Request.QueryString["type"].ToUpper())
                {
                    case "BIN":
                        type = FUTBuyBidType.BuyNow;
                        data = data.Replace("{BINBuysActive}", "active");
                        data = data.Replace("{BIDBuysActive}", "");
                        data = data.Replace("{CMBBuysActive}", "");
                        break;
                    case "BID":
                        type = FUTBuyBidType.Bid;
                        data = data.Replace("{BINBuysActive}", "");
                        data = data.Replace("{BIDBuysActive}", "active");
                        data = data.Replace("{CMBBuysActive}", "");
                        break;
                    case "CMB":
                        type = FUTBuyBidType.Combined;
                        data = data.Replace("{BINBuysActive}", "");
                        data = data.Replace("{BIDBuysActive}", "");
                        data = data.Replace("{CMBBuysActive}", "active");
                        break;
                    default:
                        type = FUTBuyBidType.Combined;
                        data = data.Replace("{BINBuysActive}", "");
                        data = data.Replace("{BIDBuysActive}", "");
                        data = data.Replace("{CMBBuysActive}", "active");
                        break;
                }
                typeString = context.Request.QueryString["type"].ToUpper();
            }

            var stringBuilderBuys = new StringBuilder();
            var logsFrom = page * _logsPerPage;
            var logsTo = _logsPerPage;
            var futBuys = new List<FUTBuy>();
            if (type == FUTBuyBidType.Combined)
            {
                futBuys = FUTLogsDatabase.GetFUTBuys(logsFrom, logsTo);
            }
            else
            {
                futBuys = FUTLogsDatabase.GetFUTBuys(logsFrom, logsTo, type);
            }
            foreach (var log in futBuys)
            {
                stringBuilderBuys.AppendLine("<tr>");
                stringBuilderBuys.AppendLine("<td>" + log.ID + "</td>");
                var dateTime = Helper.TimestampToDateTime(log.Timestamp);
                var printDate = dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString();
                stringBuilderBuys.AppendLine("<td>" + printDate + "</td>");
                stringBuilderBuys.AppendLine("<td>" + log.TradeID + "</td>");
                SimpleSearchItemModel itemData = FUTItemManager.GetItemByAssetRevisionID(log.AssetID, log.RevisionID);
                if (itemData != null)
                {
                    stringBuilderBuys.AppendLine("<td>" + itemData.GetName() + " (" + itemData.r + ")</td>");
                }
                else
                {
                    stringBuilderBuys.AppendLine("<td>" + log.AssetID + "/" + log.RevisionID + "</td>");
                }
                stringBuilderBuys.AppendLine("<td>" + log.AssetID + "/" + log.RevisionID + "</td>");
                stringBuilderBuys.AppendLine("<td>" + log.BuyNowPrice + "</td>");
                stringBuilderBuys.AppendLine("</tr>");
            }

            data = data.Replace("{BUYSDATA}", stringBuilderBuys.ToString());

            if (page == 0)
            {
                data = data.Replace("<a href=\"/buys?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "");
                if (futBuys.Count < _logsPerPage)
                {
                    data = data.Replace("<a href=\"/buys?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
                }
                data = data.Replace("<a href=\"/buys?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"/buys?type=" + typeString + "&page=" + (page + 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");


            }
            else if (futBuys.Count < _logsPerPage)
            {
                data = data.Replace("<a href=\"/buys?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"/buys?type=" + typeString + "&page=" + (page - 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/buys?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
            }
            else
            {
                data = data.Replace("<a href=\"/buys?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"/buys?type=" + typeString + "&page=" + (page - 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/buys?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"/buys?type=" + typeString + "&page=" + (page + 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");
            }

            Write200Success(context, data, "text/html");
        }

        private void HandleSellsPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/sells.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
            var page = 0;
            if (context.Request.QueryString["page"] != null)
            {
                page = int.Parse(context.Request.QueryString["page"]);
                if (page < 0)
                {
                    page = 0;
                }
            }

            FUTBuyBidType type = FUTBuyBidType.Combined;
            var typeString = "CMB";
            if (context.Request.QueryString["type"] != null)
            {
                if (context.Request.QueryString["type"].ToUpper() == "BIN")
                {
                    type = FUTBuyBidType.BuyNow;
                    data = data.Replace("{BINSellsActive}", "active");
                    data = data.Replace("{BIDSellsActive}", "");
                    data = data.Replace("{CMBSellsActive}", "");
                }
                else if (context.Request.QueryString["type"].ToUpper() == "BID")
                {
                    type = FUTBuyBidType.Bid;
                    data = data.Replace("{BINSellsActive}", "");
                    data = data.Replace("{BIDSellsActive}", "active");
                    data = data.Replace("{CMBSellsActive}", "");
                }
                else if (context.Request.QueryString["type"].ToUpper() == "CMB")
                {
                    type = FUTBuyBidType.Combined;
                    data = data.Replace("{BINSellsActive}", "");
                    data = data.Replace("{BIDSellsActive}", "");
                    data = data.Replace("{CMBSellsActive}", "active");
                }
                else
                {
                    type = FUTBuyBidType.Combined;
                    data = data.Replace("{BINSellsActive}", "");
                    data = data.Replace("{BIDSellsActive}", "");
                    data = data.Replace("{CMBSellsActive}", "active");
                }
                typeString = context.Request.QueryString["type"].ToUpper();
            }

            var stringBuilderSells = new StringBuilder();
            var logsFrom = page * _logsPerPage;
            var logsTo = _logsPerPage;
            var futSells = new List<FUTSell>();
            if (type == FUTBuyBidType.Combined)
            {
                futSells = FUTLogsDatabase.GetFUTSells(logsFrom, logsTo);
            }
            else
            {
                futSells = FUTLogsDatabase.GetFUTSells(logsFrom, logsTo, type);
            }
            
            foreach(var log in futSells)
            { 
                stringBuilderSells.AppendLine("<tr>");
                stringBuilderSells.AppendLine("<td>" + log.ID + "</td>");
                var dateTime = Helper.TimestampToDateTime(log.Timestamp);
                var printDate = dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString();
                stringBuilderSells.AppendLine("<td>" + printDate + "</td>");
                stringBuilderSells.AppendLine("<td>" + log.TradeID + "</td>");
                SimpleSearchItemModel itemData = FUTItemManager.GetItemByAssetRevisionID(log.AssetID, log.RevisionID);
                if(itemData != null)
                {
                    stringBuilderSells.AppendLine("<td>" + itemData.GetName() + " (" + itemData.r + ")</td>");
                }
                else
                {
                    stringBuilderSells.AppendLine("<td>" + log.AssetID + "/" + log.RevisionID + "</td>");
                }

                stringBuilderSells.AppendLine("<td>" + log.AssetID + "/" + log.RevisionID + "</td>");
                stringBuilderSells.AppendLine("<td>" + log.SellPrice + "</td>");
                stringBuilderSells.AppendLine("</tr>");
            }

            data = data.Replace("{SELLSDATA}", stringBuilderSells.ToString());

            if (page == 0)
            {
                data = data.Replace("<a href=\"/sells?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "");
                if (futSells.Count < _logsPerPage)
                {
                    data = data.Replace("<a href=\"/sells?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
                }
                data = data.Replace("<a href=\"/sells?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"/sells?type=" + typeString + "&page=" + (page + 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");


            }
            else if (futSells.Count < _logsPerPage)
            {
                data = data.Replace("<a href=\"/sells?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"/sells?type=" + typeString + "&page=" + (page - 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/sells?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
            }
            else
            {
                data = data.Replace("<a href=\"/sells?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"/sells?type=" + typeString + "&page=" + (page - 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/sells?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"/sells?type=" + typeString + "&page=" + (page + 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");
            }

            Write200Success(context, data, "text/html");
        }

        private void HandleBotLogsPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/botlogs.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
            var page = 0;
            if (context.Request.QueryString["page"] != null)
            {
                page = int.Parse(context.Request.QueryString["page"]);
                if (page < 0)
                {
                    page = 0;
                }
            }

            var email = "";
            if (context.Request.QueryString["email"] != null)
            {
                email = context.Request.QueryString["email"];
            }

            var stringBuilderLogs = new StringBuilder();


            var logsFrom = page * _logsPerPage;
            var logsTo = _logsPerPage;
            var futLogs = new List<FUTBotLog>();
            if(email != "")
            {
                futLogs = FUTLogsDatabase.GetFUTBotLogsByEMail(email, logsFrom, logsTo);
            }
            else
            {
                futLogs = FUTLogsDatabase.GetFUTBotLogs(logsFrom, logsTo);
            }
            
            foreach(var log in futLogs)
            {
                stringBuilderLogs.AppendLine("<tr>");
                stringBuilderLogs.AppendLine("<td>" + log.ID + "</td>");
                var link = "<a target=\"_blank\" href=\"/botlogs?email=" + log.EMail + "\">";
                stringBuilderLogs.AppendLine("<td>" + link + log.EMail +  "</a></td>"); 
                stringBuilderLogs.AppendLine("<td>" + log.Data + "</td>");
                var dateTime = Helper.TimestampToDateTime(log.Timestamp);
                var printDate = dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString();
                stringBuilderLogs.AppendLine("<td>" + printDate + "</td>");
                stringBuilderLogs.AppendLine("</tr>");
            }

            data = data.Replace("{LOGDATA}", stringBuilderLogs.ToString());

            var nextURL = "/botlogs?page=" + (page + 1);
            var previousURL = "/botlogs?page=" + (page - 1);
            if (email != "")
            {
                nextURL += "&email=" + email;
                previousURL += "&email=" + email;
            }
            if(futLogs.Count > 0)
            {
                if (page == 0)
                {
                    data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "");
                    if (futLogs.Count < _logsPerPage)
                    {
                        data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
                    }
                    data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"" + nextURL + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");


                }
                else if (futLogs.Count < _logsPerPage)
                {
                    data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"" + previousURL + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                    data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
                }
                else
                {
                    data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"" + previousURL + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                    data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"" + nextURL + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");
                }
            }
            else
            {
                data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
                data = data.Replace("<a href=\"/botlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "");
            }
            

            Write200Success(context, data, "text/html");
        }

        private void HandleExceptionLogsPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/exceptionlogs.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
            var page = 0;
            if (context.Request.QueryString["page"] != null)
            {
                page = int.Parse(context.Request.QueryString["page"]);
                if (page < 0)
                {
                    page = 0;
                }
            }

            var stringBuilderLogs = new StringBuilder();
            var logsFrom = page * _logsPerPage;
            var logsTo = _logsPerPage;
            var futLogs = FUTLogsDatabase.GetFUTExceptionLogs(logsFrom, logsTo);
            foreach (var log in futLogs)
            {
                stringBuilderLogs.AppendLine("<tr>");
                stringBuilderLogs.AppendLine("<td>" + log.ID + "</td>");
                var link = "<a target=\"_blank\" href=\"/botlogs?email=" + log.EMail + "\">";
                stringBuilderLogs.AppendLine("<td>" + link + log.EMail + "</a></td>");
                stringBuilderLogs.AppendLine("<td>" + log.Data + "</td>");
                var dateTime = Helper.TimestampToDateTime(log.Timestamp);
                var printDate = dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString();
                stringBuilderLogs.AppendLine("<td>" + printDate + "</td>");
                stringBuilderLogs.AppendLine("</tr>");
            }

            data = data.Replace("{LOGDATA}", stringBuilderLogs.ToString());

            if (page == 0)
            {
                data = data.Replace("<a href=\"/exceptionlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "");
                if (futLogs.Count < _logsPerPage)
                {
                    data = data.Replace("<a href=\"/exceptionlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
                }
                data = data.Replace("<a href=\"/exceptionlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"/exceptionlogs?page=" + (page + 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");


            }
            else if (futLogs.Count < _logsPerPage)
            {
                data = data.Replace("<a href=\"/exceptionlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"/exceptionlogs?page=" + (page - 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/exceptionlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "");
            }
            else
            {
                data = data.Replace("<a href=\"/exceptionlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>", "<a href=\"/exceptionlogs?page=" + (page - 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-left\">Previous</a>");
                data = data.Replace("<a href=\"/exceptionlogs?page=\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>", "<a href=\"/exceptionlogs?page=" + (page + 1) + "\" class=\"btn btn-sm btn-info btn-flat pull-right\">Next</a>");
            }

            Write200Success(context, data, "text/html");
        }

        private void HandleNotificationCenterPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/notificationcenter.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
            var stringBuilderOptions = new StringBuilder();


            var futNotifications = FUTLogsDatabase.GetFUTNotifications();
            futNotifications.Reverse();
            foreach (var notification in futNotifications)
            {
                var timestamp = Helper.TimestampToDateTime(notification.Timestamp);

                stringBuilderOptions.AppendLine("<tr id=\"" + notification.ID + "\">");
                stringBuilderOptions.AppendLine("<td><input type=\"checkbox\"></td>");
                var link = "<a target=\"_blank\" href=\"/botlogs?email=" + notification.EMail + "\">";
                stringBuilderOptions.AppendLine("<td>" + link + notification.EMail + "</a></td>");
                stringBuilderOptions.AppendLine("<td>" + string.Format("{0:d/M/yyyy HH:mm:ss}", timestamp) + "</td>");
                stringBuilderOptions.AppendLine("<td>" + notification.Data + "</td>");
                stringBuilderOptions.AppendLine("<td><button class=\"btn btn-block btn-primary btn-xs\" onclick=\"acknowledgeNotification('" + notification.ID + "');\">Acknowledge</button></td>"); 
                stringBuilderOptions.AppendLine("</tr>");

                stringBuilderOptions.AppendLine(Environment.NewLine);
            }


            data = data.Replace("{NOFIFICATIONS}", stringBuilderOptions.ToString());

            Write200Success(context, data, "text/html");
        }

        private void HandleBotStatisticPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/botstatistic.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());

            var lasthours = FUTLogsDatabase.GetFUTBotStatisticsLast24Hours();

            var lastProfits = FUTLogsDatabase.GetFUTProfitLogsLast24Hours();

            var totalProfit = lastProfits.Sum(x => x.Profit);

            data = data.Replace("{PROFITLAST24HRS}", totalProfit.ToString());

            if(lasthours.Count > 0)
            {
                var morrisJS = new StringBuilder();
                //morrisJS.AppendLine("var area = new Morris.Area({");
                //morrisJS.AppendLine("element: 'revenue-chart',");
                //morrisJS.AppendLine("resize: true,");
                //morrisJS.AppendLine("data: [");

                //foreach (var item in lasthours)
                //{
                //    var time = Helper.TimestampToDateTime(item.Timestamp);

                //    morrisJS.AppendLine("{y: '" + time.ToShortTimeString() + "', item1: " + (item.TotalCoins + item.TotalTradepileValue) + ", item2: " + item.TotalCoins + ", item3: " + item.TotalTradepileValue + "},");
                //}
                //morrisJS.AppendLine("],");
                //morrisJS.AppendLine("parseTime: false,");
                //morrisJS.AppendLine("xkey: 'y',");
                //morrisJS.AppendLine("ykeys: ['item1', 'item2', 'item3'],");
                //morrisJS.AppendLine("labels: ['Overall Value', 'Total Coins', 'TP Value'],");
                //morrisJS.AppendLine("lineColors: ['#a0d0e0', '#3c8dbc', '#3c8dbc'],");
                //morrisJS.AppendLine("hideHover: 'auto',");
                //morrisJS.AppendLine("ymin : " + minRounded);
                //morrisJS.AppendLine("});");

                morrisJS.AppendLine(" ");
                morrisJS.AppendLine(" ");

                morrisJS.AppendLine("var area = new Morris.Line({");
                morrisJS.AppendLine("element: 'line-chart',");
                morrisJS.AppendLine("resize: true,");
                morrisJS.AppendLine("data: [");

                foreach (var item in lasthours)
                {
                    var time = Helper.TimestampToDateTime(item.Timestamp);

                    morrisJS.AppendLine("{y: '" + time.ToShortTimeString() + "', item1: " + (item.TotalCoins + item.TotalTradepileValue) + ", item2: " + item.TotalCoins + ", item3: " + item.TotalTradepileValue + "},");
                }
                morrisJS.AppendLine("],");
                morrisJS.AppendLine("parseTime: false,");
                morrisJS.AppendLine("xkey: 'y',");
                morrisJS.AppendLine("ykeys: ['item1', 'item2', 'item3'],");
                morrisJS.AppendLine("labels: ['Overall Value', 'Total Coins', 'TP Value'],");
                morrisJS.AppendLine("lineColors: ['#a0d0e0', '#3c8dbc', '#3c8dbc'],");
                morrisJS.AppendLine("hideHover: 'auto',");
                //morrisJS.AppendLine("ymin : " + minRounded);
                morrisJS.AppendLine("ymin : " + "0");
                morrisJS.AppendLine("});");


                data = data.Replace("{JSONDATA}", morrisJS.ToString());
            }
            else
            {
                data = data.Replace("{JSONDATA}", "");
            }
            
        

            Write200Success(context, data, "text/html");
        }

        private void AcknowledgeNotificationRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var notification = int.Parse(parameters["notification"]);

                FUTLogsDatabase.RemoveFUTNotification(notification);

                Write200Success(context, "true", "application/json");
            }
        }

        private void AcknowledgeNotificationArrayRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var notifications = parameters["notification"].Split(',');

                foreach(var not in notifications)
                {
                    var id = int.Parse(not);
                    FUTLogsDatabase.RemoveFUTNotification(id);
                }
                
                

                Write200Success(context, "true", "application/json");
            }
        }
        #endregion

        #region Statistic

        private void HandleItemStatisticPageOld(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/itemstatistic.html");

            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if(nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());

            var stringBuilderOptions = new StringBuilder();


            var listItems = ItemListManager.GetFUTListItems();

            var buysDictionary = new Dictionary<string, List<FUTBuy>>();
            var sellsDictionary = new Dictionary<string, List<FUTSell>>();
            var tradepileDictionary = new Dictionary<string, List<AuctionInfo>>();

            var allBuys = FUTLogsDatabase.GetFUTBuys();
            var allSells = FUTLogsDatabase.GetFUTSells();
            var allTPItems = BotManager.GetTradepileItems();

            foreach (var buy in allBuys)
            {
                if (!buysDictionary.ContainsKey(buy.AssetID + "|" + buy.RevisionID))
                {
                    buysDictionary.Add(buy.AssetID + "|" + buy.RevisionID, new List<FUTBuy>());
                }
                buysDictionary[buy.AssetID + "|" + buy.RevisionID].Add(buy);
            }
            foreach (var sell in allSells)
            {
                if (!sellsDictionary.ContainsKey(sell.AssetID + "|" + sell.RevisionID))
                {
                    sellsDictionary.Add(sell.AssetID + "|" + sell.RevisionID, new List<FUTSell>());

                }
                sellsDictionary[sell.AssetID + "|" + sell.RevisionID].Add(sell);
            }

            foreach (var item in allTPItems)
            {
                var assetID = ResourceIDManager.GetAssetID(item.itemData.resourceId);
                var revID = ResourceIDManager.GetRevID(item.itemData.resourceId);
                if (!tradepileDictionary.ContainsKey(assetID + "|" + revID))
                {
                    tradepileDictionary.Add(assetID + "|" + revID, new List<AuctionInfo>());

                }
                tradepileDictionary[assetID + "|" + revID].Add(item);
            }

            foreach (var item in listItems)
            {
                var expectedProfit = 0;
                
                if (!buysDictionary.ContainsKey(item.AssetID + "|" + item.RevisionID))
                {
                    buysDictionary.Add(item.AssetID + "|" + item.RevisionID, new List<FUTBuy>());
                }
                if (!sellsDictionary.ContainsKey(item.AssetID + "|" + item.RevisionID))
                {
                    sellsDictionary.Add(item.AssetID + "|" + item.RevisionID, new List<FUTSell>());
                }
                if (!tradepileDictionary.ContainsKey(item.AssetID + "|" + item.RevisionID))
                {
                    tradepileDictionary.Add(item.AssetID + "|" + item.RevisionID, new List<AuctionInfo>());
                }
                else
                {
                    foreach(var tpItem in tradepileDictionary[item.AssetID + "|" + item.RevisionID])
                    {
                        expectedProfit += (int)(tpItem.buyNowPrice * 0.95);
                    }
                }
                var itemsCountTP = tradepileDictionary[item.AssetID + "|" + item.RevisionID].Count;
                var itemsCountCalc = buysDictionary[item.AssetID + "|" + item.RevisionID].Count - sellsDictionary[item.AssetID + "|" + item.RevisionID].Count;

                var statistic = new SimpleItemStatistic(buysDictionary[item.AssetID + "|" + item.RevisionID], sellsDictionary[item.AssetID + "|" + item.RevisionID]);

                var assetItem = FUTItemManager.GetItemByAssetRevisionID(item.AssetID, item.RevisionID);
                var additional = assetItem == null ? item.RevisionID : assetItem.r;
                stringBuilderOptions.AppendLine("<tr>");
                var link = "<a target=\"_blank\" href=\"/profitlogs?assetid=" + item.AssetID + "&revid=" + item.RevisionID + "\">";
                if (item != null)
                {
                    stringBuilderOptions.AppendLine("<td>" + item.Name + " (" + additional + ")</td>");
                }
                else
                {
                    stringBuilderOptions.AppendLine("<td>" + item.Name + " (" + additional + ")</td>");
                }
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalProfit + "</td>");
                stringBuilderOptions.AppendLine("<td>" + expectedProfit + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.AverageBuyprice + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.AverageSellprice + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.AverageProfit + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalBuys + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalSells + "</td>");
                stringBuilderOptions.AppendLine("<td>" + itemsCountTP + "/" + itemsCountCalc + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalBuyValue + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalSellValue + "</td>");
                stringBuilderOptions.AppendLine("</tr>");

                stringBuilderOptions.AppendLine(Environment.NewLine);
            }

            data = data.Replace("{ITEMSTATISTICS}", stringBuilderOptions.ToString());

            Write200Success(context, data, "text/html");
        }

        private void HandleItemStatisticPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/itemstatistic.html");

            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());

            var stringBuilderOptions = new StringBuilder();


            var listItems = ItemListManager.GetFUTListItems();

            var allProfits = FUTLogsDatabase.GetFUTProfitLogs();

            var allTPItems = BotManager.GetTradepileItems();
            var tradepileDictionary = new Dictionary<string, List<AuctionInfo>>();
            foreach (var item in allTPItems)
            {
                var assetID = ResourceIDManager.GetAssetID(item.itemData.resourceId);
                var revID = ResourceIDManager.GetRevID(item.itemData.resourceId);
                if (!tradepileDictionary.ContainsKey(assetID + "|" + revID))
                {
                    tradepileDictionary.Add(assetID + "|" + revID, new List<AuctionInfo>());

                }
                tradepileDictionary[assetID + "|" + revID].Add(item);
            }

            foreach (var item in listItems)
            {
                var expectedProfit = 0;
                if (!tradepileDictionary.ContainsKey(item.AssetID + "|" + item.RevisionID))
                {
                    tradepileDictionary.Add(item.AssetID + "|" + item.RevisionID, new List<AuctionInfo>());
                }
                else
                {
                    foreach (var tpItem in tradepileDictionary[item.AssetID + "|" + item.RevisionID])
                    {
                        expectedProfit += (int)(tpItem.buyNowPrice * 0.95);
                    }
                }

                var profits = allProfits.Where(x => x.AssetID == item.AssetID && x.RevisionID == item.RevisionID).ToList();

                var itemsCountTP = profits.Where(x => x.SellTimestamp == 0).Count();
                

                var statistic = new SimpleItemStatistic(profits);

                var assetItem = FUTItemManager.GetItemByAssetRevisionID(item.AssetID, item.RevisionID);
                var additional = assetItem?.r ?? item.RevisionID;
                stringBuilderOptions.AppendLine("<tr>");
                var link = "<a target=\"_blank\" href=\"/profitlogs?assetid=" + item.AssetID + "&revid=" + item.RevisionID + "\">";
                stringBuilderOptions.AppendLine("<td>" + link + item.Name + " (" + additional + ")</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalProfit + "</td>");
                stringBuilderOptions.AppendLine("<td>" + expectedProfit + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.AverageBuyprice + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.AverageSellprice + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.AverageProfit + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalBuys + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalSells + "</td>");
                stringBuilderOptions.AppendLine("<td>" + itemsCountTP + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalBuyValue + "</td>");
                stringBuilderOptions.AppendLine("<td>" + statistic.TotalSellValue + "</td>");
                stringBuilderOptions.AppendLine("</tr>");

                stringBuilderOptions.AppendLine(Environment.NewLine);
            }

            data = data.Replace("{ITEMSTATISTICS}", stringBuilderOptions.ToString());

            Write200Success(context, data, "text/html");
        }

        private void HandleAccountStatisticPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            var data = File.ReadAllText("Autobuyer.web/accountstatistic.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
            var stringBuilderOptions = new StringBuilder();


            var futClients = BotManager.GetFutClients();

            foreach (var futClient in futClients)
            {
                var lastAction = Helper.TimestampToDateTime(futClient.FUTAccountStatistic.LastActionTimestamp);

                stringBuilderOptions.AppendLine("<tr>");
                var link = "<a target=\"_blank\" href=\"/botlogs?email=" + futClient.FUTAccount.EMail + "\">";
                stringBuilderOptions.AppendLine("<td>" + link + futClient.FUTAccount.EMail + "</a></td>");
                stringBuilderOptions.AppendLine("<td>" + futClient.Coins + "</td>");
                stringBuilderOptions.AppendLine("<td>" + string.Format("{0:d/M/yyyy HH:mm:ss}", lastAction) + "</td>");
                stringBuilderOptions.AppendLine("<td>" + futClient.FUTAccountStatistic.LastActionData + "</td>");
                stringBuilderOptions.AppendLine("<td>" + futClient.LoggedIn + "</td>");
                stringBuilderOptions.AppendLine("<td>" + futClient.FUTAccountStatistic.TotalLogins + "</td>");
                stringBuilderOptions.AppendLine("<td>" + futClient.FUTAccountStatistic.TradepileCount + "</td>");
                stringBuilderOptions.AppendLine("<td>" + futClient.LogicRunningReal + "</td>");
                if(futClient.LogicRunningReal)
                {
                    stringBuilderOptions.AppendLine("<td><button class=\"btn btn-block btn-danger btn-xs\" onclick=\"stopAccount('" + futClient.FUTAccount.EMail + "');\">Stop</button></td>");
                }
                else
                {
                    stringBuilderOptions.AppendLine("<td><button class=\"btn btn-block btn-success btn-xs\" onclick=\"startAccount('" + futClient.FUTAccount.EMail + "');\">Start</button></td>");
                }
                stringBuilderOptions.AppendLine("</tr>");

                stringBuilderOptions.AppendLine(Environment.NewLine);
            }


            data = data.Replace("{ACCOUNTSTATISTICS}", stringBuilderOptions.ToString());

            Write200Success(context, data, "text/html");
        }

        private void StartSingleAccountRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var account = parameters["account"];

                BotManager.StartBot(account);

                Write200Success(context, "true", "application/json");
            }
        }

        private void StopSingleAccountRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var account = parameters["account"];

                BotManager.StopBot(account);

                Write200Success(context, "true", "application/json");
            }
        }
        #endregion

        #region  Settings
        private void HandleGeneralSettingsPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (!HasAccessToPage(context, WebAccessRole.Moderator))
            {
                context.Response.Redirect("/?access=no");
                context.Response.Close();
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                var data = File.ReadAllText("Autobuyer.web/generalsettings.html");
                var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
                if (nofiticationCount <= 0)
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", "");
                }
                else
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
                }
                data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
                FUTSettings.Update();
                data = data.Replace("name=\"rpmMin\" value=\"\"", "name=\"rpmMin\" value=\"" + FUTSettings.RoundsPerMinuteMin +  "\"");
                data = data.Replace("name=\"rpmMax\" value=\"\"", "name=\"rpmMax\" value=\"" + FUTSettings.RoundsPerMinuteMax + "\"");
                data = data.Replace("name=\"rpmMinSearch\" value=\"\"", "name=\"rpmMinSearch\" value=\"" + FUTSettings.RoundsPerMinuteMinSearch + "\"");
                data = data.Replace("name=\"rpmMaxSearch\" value=\"\"", "name=\"rpmMaxSearch\" value=\"" + FUTSettings.RoundsPerMinuteMaxSearch + "\"");
                data = data.Replace("name=\"pauseRelogs\" value=\"\"", "name=\"pauseRelogs\" value=\"" + FUTSettings.PauseBetweenRelogs + "\"");
                data = data.Replace("name=\"tradepileCheckTimes\" value=\"\"", "name=\"tradepileCheckTimes\" value=\"" + FUTSettings.TradepileCheck + "\"");
                data = data.Replace("name=\"counterBase\" value=\"\"", "name=\"counterBase\" value=\"" + FUTSettings.Counter + "\"");
                data = data.Replace("name=\"priceCorrectionPercentage\" value=\"\"", "name=\"priceCorrectionPercentage\" value=\"" + FUTSettings.PriceCorrectionPercentage + "\"");
                data = data.Replace("name=\"pricecheckTime\" value=\"\"", "name=\"pricecheckTime\" value=\"" + FUTSettings.PriceCheckTimes + "\"");
                data = data.Replace("name=\"stopBuying\"", "name=\"stopBuying\" " + (FUTSettings.StopBuying ? "checked" : ""));
                data = data.Replace("name=\"relistWithOldPrice\"", "name=\"relistWithOldPrice\" " + (FUTSettings.RelistWithOldPrice ? "checked" : ""));
                data = data.Replace("name=\"useLastPriceChecks\" value=\"\"", "name=\"useLastPriceChecks\" value=\"" + FUTSettings.UseLastPriceChecks + "\"");
                data = data.Replace("name=\"discardEverything\"", "name=\"discardEverything\" " + (FUTSettings.DiscardEverything ? "checked" : ""));
                data = data.Replace("name=\"useMobileLogin\"", "name=\"useMobileLogin\" " + (FUTSettings.LoginMethod == LoginMethod.Mobile ? "checked" : ""));
                data = data.Replace("name=\"useLoginSwitch\"", "name=\"useLoginSwitch\" " + (FUTSettings.UseLoginSwitch? "checked" : ""));
                data = data.Replace("name=\"useBidSwitch\"", "name=\"useBidSwitch\" " + (FUTSettings.UseBidSwitch ? "checked" : ""));
                data = data.Replace("name=\"useRandomRequests\"", "name=\"useRandomRequests\" " + (FUTSettings.UseRandomRequests ? "checked" : ""));
                data = data.Replace("name=\"oneParallelLogin\"", "name=\"oneParallelLogin\" " + (FUTSettings.OneParallelLogin ? "checked" : ""));
                data = data.Replace("name=\"watchlistCheckTimes\" value=\"\"", "name=\"watchlistCheckTimes\" value=\"" + FUTSettings.WatchlistCheck + "\"");
                data = data.Replace("name=\"expiredTimer\" value=\"\"", "name=\"expiredTimer\" value=\"" + FUTSettings.ExpiredTimer + "\"");

                data = data.Replace("name=\"waitAfterBuy\" value=\"\"", "name=\"waitAfterBuy\" value=\"" + FUTSettings.WaitAfterBuy + "\"");
                data = data.Replace("name=\"maxCardsPerDay\" value=\"\"", "name=\"maxCardsPerDay\" value=\"" + FUTSettings.MaxCardsPerDay + "\"");
                Write200Success(context, data, "text/html");
            }

            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                FUTSettings.RoundsPerMinuteMin = int.Parse(parameters["rpmMin"]);
                FUTSettings.RoundsPerMinuteMax = int.Parse(parameters["rpmMax"]);
                FUTSettings.RoundsPerMinuteMinSearch = int.Parse(parameters["rpmMinSearch"]);
                FUTSettings.RoundsPerMinuteMaxSearch = int.Parse(parameters["rpmMaxSearch"]);
                FUTSettings.PauseBetweenRelogs = int.Parse(parameters["pauseRelogs"]);
                FUTSettings.TradepileCheck = int.Parse(parameters["tradepileCheckTimes"]);
                FUTSettings.Counter = int.Parse(parameters["counterBase"]);
                FUTSettings.PriceCorrectionPercentage = int.Parse(parameters["priceCorrectionPercentage"]);
                FUTSettings.PriceCheckTimes = int.Parse(parameters["pricecheckTime"]);
                FUTSettings.StopBuying = parameters["stopBuying"] == null ? false : true;
                FUTSettings.RelistWithOldPrice = parameters["relistWithOldPrice"] == null ? false : true;
                FUTSettings.DiscardEverything = parameters["discardEverything"] == null ? false : true;
                FUTSettings.LoginMethod = parameters["useMobileLogin"] == null ? LoginMethod.Web : LoginMethod.Mobile;
                FUTSettings.UseLoginSwitch = parameters["useLoginSwitch"] == null ? false : true;
                FUTSettings.UseBidSwitch = parameters["useBidSwitch"] == null ? false : true;
                FUTSettings.UseRandomRequests = parameters["useRandomRequests"] == null ? false : true;
                FUTSettings.OneParallelLogin = parameters["oneParallelLogin"] == null ? false : true;
                FUTSettings.WatchlistCheck = int.Parse(parameters["watchlistCheckTimes"]);
                FUTSettings.ExpiredTimer = int.Parse(parameters["expiredTimer"]);
                FUTSettings.WaitAfterBuy = int.Parse(parameters["waitAfterBuy"]);
                FUTSettings.MaxCardsPerDay = int.Parse(parameters["maxCardsPerDay"]);
                FUTSettings.UseLastPriceChecks = int.Parse(parameters["useLastPriceChecks"]);
                FUTSettings.SaveChanges();
                FUTSettings.Update();

                context.Response.Redirect("/generalsettings");
                context.Response.Close();
                return;
            }
        }

        private void HandleManageListPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (!HasAccessToPage(context, WebAccessRole.Moderator))
            {
                context.Response.Redirect("/?access=no");
                context.Response.Close();
                return;
            }
            var data = File.ReadAllText("Autobuyer.web/managelist.html");
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }
            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
            var stringBuilderOptions = new StringBuilder();

            var itemsInList = ItemListManager.GetFUTListItems();
            foreach(var item in itemsInList)
            {
                var itemX = FUTItemManager.GetItemByAssetRevisionID(item.AssetID, item.RevisionID);
                var itemName = "";
                var rr = item.RevisionID;
                if (itemX != null)
                {
                    rr = itemX.r;
                }

                stringBuilderOptions.AppendLine("<tr>");
                var link = "<a target=\"_blank\" href=\"/profitlogs?assetid=" + item.AssetID + "&revid=" + item.RevisionID + "\">";
                stringBuilderOptions.AppendLine("<td style=\"vertical-align: middle;\">" + link + item.Name + " (" + rr + ")</a></td>");
                stringBuilderOptions.AppendLine("<td><input min=\"0\" type=\"number\" class=\"form-control\" id=\"staticBuyPercent_" + item.ID + "\" name=\"staticBuyPercent_" + item.ID + "\" value=\"" + item.StaticBuyPercent +  "\" ></td>");
                stringBuilderOptions.AppendLine("<td><input min=\"0\" type=\"number\" class=\"form-control\" id=\"variableBuyPercent_" + item.ID + "\" name=\"variableBuyPercent_" + item.ID + "\" value=\"" + item.VariableBuyPercent + "\" ></td>");
                stringBuilderOptions.AppendLine("<td><input min=\"0\" type=\"number\" class=\"form-control\" id=\"buyPercentStep_" + item.ID + "\" name=\"buyPercentStep_" + item.ID + "\" value=\"" + item.BuyPercentStep + "\" ></td>");
                stringBuilderOptions.AppendLine("<td><input min=\"0\" type=\"number\" class=\"form-control\" id=\"counter_" + item.ID + "\" name=\"counter_" + item.ID + "\" value=\"" + item.Counter + "\" ></td>");
                stringBuilderOptions.AppendLine("<td><input min=\"0\" type=\"number\" class=\"form-control\" id=\"sellPercent_" + item.ID + "\" name=\"sellPercent_" + item.ID + "\" value=\"" + item.SellPercent + "\" ></td>");
                stringBuilderOptions.AppendLine("<td><input min=\"0\" type=\"number\" class=\"form-control\" id=\"buyPrice_" + item.ID + "\" name=\"buyPrice_" + item.ID + "\" value=\"" + item.BuyPrice + "\" ></td>");
                stringBuilderOptions.AppendLine("<td><input min=\"0\" type=\"number\" class=\"form-control\" id=\"sellPrice_" + item.ID + "\" name=\"sellPrice_" + item.ID + "\" value=\"" + item.SellPrice + "\" ></td>");
                if(item.IgnorePriceCheck)
                {
                    stringBuilderOptions.AppendLine("<td style=\"vertical-align:middle\"><input type=\"checkbox\" name=\"ignorePriceCheck_" + item.ID + "\" id=\"ignorePriceCheck_" + item.ID + "\" checked=\"true\"></td>");
                }
                else
                {
                    stringBuilderOptions.AppendLine("<td style=\"vertical-align:middle\"><input type=\"checkbox\" name=\"ignorePriceCheck_" + item.ID + "\" id=\"ignorePriceCheck_" + item.ID + "\"></td>");
                }
                if (item.BuyItem)
                {
                    stringBuilderOptions.AppendLine("<td style=\"vertical-align:middle\"><input type=\"checkbox\" name=\"buyItem_" + item.ID + "\" id=\"buyItem_" + item.ID + "\" checked=\"true\"></td>");
                }
                else
                {
                    stringBuilderOptions.AppendLine("<td style=\"vertical-align:middle\"><input type=\"checkbox\" name=\"buyItem_" + item.ID + "\" id=\"buyItem_" + item.ID + "\"></td>");
                }
                if (item.Discard)
                {
                    stringBuilderOptions.AppendLine("<td style=\"vertical-align:middle\"><input type=\"checkbox\" name=\"discardItem_" + item.ID + "\" id=\"discardItem_" + item.ID + "\" checked=\"true\"></td>");
                }
                else
                {
                    stringBuilderOptions.AppendLine("<td style=\"vertical-align:middle\"><input type=\"checkbox\" name=\"discardItem_" + item.ID + "\" id=\"discardItem_" + item.ID + "\"></td>");
                }
                var time = Helper.TimestampToDateTime(item.LastPriceCheck);
                var formattedTime = time.ToShortDateString() + " " + time.ToShortTimeString();
                stringBuilderOptions.AppendLine("<td id=\"lastPriceCheck_" + item.ID + "\" name=\"lastPriceCheck_" + item.ID + "\">" + formattedTime + "</td>");
                stringBuilderOptions.AppendLine("<td><button class=\"btn btn-block btn-success\" onclick=\"resetPriceCheck('" + item.ID + "');\">Reset PriceCheck</button></td>");
                stringBuilderOptions.AppendLine("<td><button class=\"btn btn-block btn-success\" onclick=\"removeItemFromList('" + item.ID + "');\">Remove</button></td>");
                stringBuilderOptions.AppendLine("<td><button class=\"btn btn-block btn-success\" onclick=\"saveItem('" + item.ID + "');\">Save</button></td>");
                stringBuilderOptions.AppendLine(Environment.NewLine);
            }

            data = data.Replace("{ITEMLIST}", stringBuilderOptions.ToString());
            data = data.Replace("{ITEMCOUNT}", itemsInList.Count.ToString());

            var errorCode = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("error");
            if(!string.IsNullOrEmpty(errorCode))
            {
                data = data.Replace("id=\"errorBox\" style=\"display:none;\"", "id=\"errorBox\"");
                if (errorCode == "1")
                {
                    data = data.Replace("{ERRORMESSAGE}", "The Item already exists in your list!");
                }
            }
            Write200Success(context, data, "text/html");
            
        }

        private void HandleBotManagerPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (!HasAccessToPage(context, WebAccessRole.Moderator))
            {
                context.Response.Redirect("/?access=no");
                context.Response.Close();
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                var data = File.ReadAllText("Autobuyer.web/botmanager.html");
                var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
                if (nofiticationCount <= 0)
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", "");
                }
                else
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
                }
                data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
                var accountsRunning = BotManager.GetFutClients().Where(x => x.LogicRunningReal).Count();
                var accountstotal = BotManager.GetFutClients().Count;
                data = data.Replace("{RUNNINGACCOUNTS}", accountsRunning + "/" + accountstotal);

                Write200Success(context, data, "text/html");
            }

            if(context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                if(parameters["startbot"] != null)
                {
                    if(parameters["startbot"] == "true")
                    {
                        BotManager.StartAllBots();
                    }
                }

                if (parameters["stopbot"] != null)
                {
                    if (parameters["stopbot"] == "true")
                    {
                        BotManager.StopAllBots();
                    }
                }

                if (parameters["startnotrunningbot"] != null)
                {
                    if (parameters["startnotrunningbot"] == "true")
                    {
                        BotManager.StartAllNotRunningBots();
                    }
                }

                Write200Success(context, "success", "application/json");
            }

        }
        #endregion

        #region  Manage Accounts
        private void HandleAddAccountPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (!HasAccessToPage(context, WebAccessRole.Owner))
            {
                context.Response.Redirect("/?access=no");
                context.Response.Close();
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                var data = File.ReadAllText("Autobuyer.web/addaccount.html");
                var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
                if (nofiticationCount <= 0)
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", "");
                }
                else
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
                }
                data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
                var platformID = int.Parse(ConfigurationManager.AppSettings["Platform"]);
                if (platformID == 0)
                {
                    data = data.Replace("<option value=\"1\">Playstation 3</option>", "");
                    data = data.Replace("<option value=\"2\">Xbox 360</option>", "");
                    data = data.Replace("<option value=\"3\">Xbox One</option>", "");
                }
                else if (platformID == 1)
                {
                    data = data.Replace("<option value=\"0\">Playstation 4</option>", "");
                    data = data.Replace("<option value=\"2\">Xbox 360</option>", "");
                    data = data.Replace("<option value=\"3\">Xbox One</option>", "");
                }
                else if (platformID == 2)
                {
                    data = data.Replace("<option value=\"0\">Playstation 4</option>", "");
                    data = data.Replace("<option value=\"1\">Playstation 3</option>", "");
                    data = data.Replace("<option value=\"3\">Xbox One</option>", "");
                }
                else if (platformID == 3)
                {
                    data = data.Replace("<option value=\"0\">Playstation 4</option>", "");
                    data = data.Replace("<option value=\"1\">Playstation 3</option>", "");
                    data = data.Replace("<option value=\"2\">Xbox 360</option>", "");
                }

                var errorCode = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("error");
                if (!string.IsNullOrEmpty(errorCode))
                {
                    data = data.Replace("id=\"errorBox\" style=\"display:none;\"", "id=\"errorBox\"");
                    if (errorCode == "1")
                    {
                        data = data.Replace("{ERRORMESSAGE}", "The Account already exists in your list!");
                    }
                }

                Write200Success(context, data, "text/html");
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                if(FUTAccountsDatabase.GetFUTAccountByEMail(parameters["accountEMail"].ToLower()) != null)
                {
                    context.Response.Redirect("/addaccount?error=1");
                    context.Response.Close();
                }
                var futAccount = new FUTAccount();
                futAccount.EMail = parameters["accountEMail"];
                futAccount.EMailPassword = parameters["accountEMailPassword"];
                futAccount.Password = parameters["accountPassword"];
                futAccount.Platform = int.Parse(parameters["accountPlatform"]);
                futAccount.SecurityAnswer = parameters["accountSecurityAnswer"];
                futAccount.BackupCode = parameters["accountBackupCode"];

                var proxy = parameters["accountProxyServer"] + ";" + parameters["accountProxyUsername"] + ";" +  parameters["accountProxyPassword"];
                futAccount.ProxyData = proxy;
                FUTAccountsDatabase.AddFUTAccount(futAccount);
                BotManager.AddBot(futAccount.EMail);

                context.Response.Redirect("/addaccount");
                context.Response.Close();
            }

        }

        private void HandleEditAccountPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (!HasAccessToPage(context, WebAccessRole.Owner))
            {
                context.Response.Redirect("/?access=no");
                context.Response.Close();
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                var data = File.ReadAllText("Autobuyer.web/editaccount.html");
                var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
                if (nofiticationCount <= 0)
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", "");
                }
                else
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
                }
                data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());
                var stringBuilderAccounts = new StringBuilder();

                var futAccounts = FUTAccountsDatabase.GetFUTAccounts();
                foreach (var account in futAccounts)
                {
                    stringBuilderAccounts.AppendLine("<option value='" + account.EMail + "'>" + account.EMail + "</option>");
                }

                data = data.Replace("{PLAYERLISTOPTIONS}", stringBuilderAccounts.ToString());

                Write200Success(context, data, "text/html");
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var account = parameters["selectEditAccount"];
                if (parameters["removeAccount"] != null)
                {
                    FUTAccountsDatabase.RemoveFUTAccountByEMail(account);
                    BotManager.RemoveBot(account);
                }
                if(parameters["saveAccount"] != null)
                {
                    var proxyData = parameters["accountProxyServer"] + ";" + parameters["accountProxyUsername"] + ";" + parameters["accountProxyPassword"];
                    FUTAccountsDatabase.UpdateFUTAccountProxyByEMail(account, proxyData);
                    FUTAccountsDatabase.UpdateFUTAccountPasswordByEMail(account, parameters["accountPassword"]);
                    FUTAccountsDatabase.UpdateFUTAccountBackupCode(account, parameters["accountBackupCode"]);
                    FUTAccountsDatabase.UpdateFUTAccountEMailPasswordByEMail(account, parameters["accountEMailPassword"]);
                    FUTAccountsDatabase.UpdateFUTAccountSecurityAnswerByEMail(account, parameters["accountSecurityAnswer"]);
                    BotManager.UpdateBot(account);
                }

                context.Response.Redirect("/editaccount");
                context.Response.Close();
            }

        }
        #endregion

        #region  XML/JSON Requests
        private void HandleAddItemRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var assetID = int.Parse(parameters["addAssetID"]);
                var revID = int.Parse(parameters["revID"]);
                var itemType = int.Parse(parameters["itemType"]);

                if(ItemListManager.ItemExistsInList(assetID, revID))
                {
                    context.Response.Redirect("/managelist?error=1");
                    context.Response.Close();
                    return;
                }
                
                var newPlayerObject = new FUTListItem(assetID);
                newPlayerObject.RevisionID = revID;
                newPlayerObject.Type = (FUTSearchParameterType)itemType;

                ItemListManager.InsertFUTListItem(newPlayerObject);

                context.Response.Redirect("/managelist");
                context.Response.Close();
                return;
            }
        }

        private void HandleSearchForItemRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var item = parameters["item"];

                var items = FUTItemManager.GetMatchingItems(item);

                var userFriendlyItems = new List<UserFriendlyItemObject>();
                foreach(var p in items)
                {
                    var converted = new UserFriendlyItemObject();
                    converted.AssetID = p.id;
                    converted.Rating = p.r;
                    converted.Name = p.GetName();
                    converted.RevisionID = p.RevisionID;
                    converted.Type = p.Type;
                    userFriendlyItems.Add(converted);
                }

                var itemsJson = JsonConvert.SerializeObject(userFriendlyItems);

                Write200Success(context, itemsJson, "application/json");
            }
        }

        private void HandleResetPriceCheckRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var databaseID = int.Parse(parameters["item"]);

                var itemsMatching = ItemListManager.GetFUTListItems().Where(x => x.ID == databaseID).FirstOrDefault();
                itemsMatching.LastPriceCheck = 0;
                itemsMatching.PriceChecking = false;
                itemsMatching.SaveChanges();

                var itemsMatchingNew = ItemListManager.GetFUTListItems().Where(x => x.ID == databaseID).FirstOrDefault();
                var itemsJson = JsonConvert.SerializeObject(itemsMatchingNew);

                Write200Success(context, itemsJson, "application/json");
            }
        }

        private void HandleRemoveItemFromListRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var databaseID = int.Parse(parameters["item"]);

                ItemListManager.RemoveFUTListItem(databaseID);

                context.Response.Redirect("/managelist");
                context.Response.Close();
                return;
            }
        }

        private void HandleGetAccountInformationRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var accountEMail = parameters["account"];

                var accountsMatching = FUTAccountsDatabase.GetFUTAccountByEMail(accountEMail);

                var accountsJson = JsonConvert.SerializeObject(accountsMatching);

                Write200Success(context, accountsJson, "application/json");
            }
        }

        private void HandleSaveItemRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);


                var databaseID = int.Parse(parameters["itemDatabaseID"]);
               
                var item = ItemListManager.GetFUTListItems().Where(x => x.ID == databaseID).FirstOrDefault();
                if (item != null)
                {
                    item.StaticBuyPercent = int.Parse(parameters["staticBuyPercent"]);
                    item.VariableBuyPercent = int.Parse(parameters["variableBuyPercent"]);
                    item.BuyPercentStep = int.Parse(parameters["buyPercentStep"]);
                    item.SellPercent = int.Parse(parameters["sellPercent"]);
                    item.Counter = int.Parse(parameters["counter"]);
                    item.BuyPrice = int.Parse(parameters["buyPrice"]);
                    item.SellPrice = int.Parse(parameters["sellPrice"]);
                    item.IgnorePriceCheck = parameters["ignorePriceCheck"] == "false" ? false : true;
                    item.BuyItem = parameters["buyItem"] == "false" ? false : true;
                    item.Discard = parameters["discardItem"] == "false" ? false : true;
                    item.SaveChanges();
                }


                var itemJson = JsonConvert.SerializeObject(item);

                Write200Success(context, itemJson, "application/json");

            }
        }
        #endregion

        private void HandleUpdateItemsJsonRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            try
            {
                XMLToJSONPlayerParser.Update();
                FUTItemManager.ResetItems();
                Write200Success(context, "Update Success", "text/html");
            }
            catch(Exception e)
            {
                Write200Success(context, "Update Failed: \r\n\r\n" + e, "text/html");
            }

           
        }

        private void HandleResetPricechecksRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                ItemListManager.ResetPriceCheckEverywhere();

                Write200Success(context, "success", "application/json");
            }
        }

        #region Muling
        private void HandleMulePage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (!HasAccessToPage(context, WebAccessRole.Administrator))
            {
                context.Response.Redirect("/?access=no");
                context.Response.Close();
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);
                if (parameters["action"] != null && parameters["email"] != null)
                {
                    var email = parameters["email"];
                    var muleAccount = MuleManager.GetMuleClientByEMail(email);
                    if(muleAccount == null)
                    {
                        context.Response.Redirect("/mule");
                        context.Response.Close();
                        return;
                    }
                    if (parameters["action"] == "start")
                    {
                        muleAccount.StartMule();
                    }
                    else if(parameters["action"] == "stop")
                    {
                        muleAccount.StopMule();
                    }
                    
                    context.Response.Redirect("/mulesession?email=" + email);
                    context.Response.Close();
                }
                else if(parameters["importMulingsessions"] != null)
                {
                    var data = parameters["importData"];

                    var lines = data.Split("\r\n".ToCharArray());

                    foreach(var line in lines)
                    {
                        if(line.Trim() == "" || !line.Contains(";"))
                        {
                            continue;
                        }
                        var accountData = line.Split(';');
                        if(accountData.Count() != 11)
                        {
                            continue;
                        }
                        var futAccount = new FUTAccount();
                        futAccount.EMail = accountData[0];
                        futAccount.EMailPassword = accountData[2];
                        futAccount.Password = accountData[1];
                        futAccount.BackupCode = accountData[4];
                        futAccount.Platform = int.Parse(accountData[5]);
                        futAccount.SecurityAnswer = accountData[3];

                        var proxy = accountData[6] + ";" + accountData[7] + ";" + accountData[8];
                        futAccount.ProxyData = proxy;
                        var muleClient = new MuleClient(futAccount);
                        muleClient.MuleVolume = int.Parse(accountData[9]);
                        muleClient.MinimumCoinsOnAccount = int.Parse(accountData[10]);
                        MuleManager.AddMuleClient(muleClient);
                    }
                    context.Response.Redirect("/mule");
                    context.Response.Close();
                }
                else if (parameters["addMulingsession"] != null)
                {
                    var futAccount = new FUTAccount();
                    futAccount.EMail = parameters["accountEMail"];
                    futAccount.EMailPassword = parameters["accountEMailPassword"];
                    futAccount.Password = parameters["accountPassword"];
                    futAccount.BackupCode = parameters["accountBackupCode"];
                    futAccount.Platform = int.Parse(parameters["accountPlatform"]);
                    futAccount.SecurityAnswer = parameters["accountSecurityanswer"];

                    var proxy = parameters["accountProxyServer"] + ";" + parameters["accountProxyUsername"] + ";" + parameters["accountProxyPassword"];
                    futAccount.ProxyData = proxy;
                    var muleClient = new MuleClient(futAccount);
                    muleClient.MuleVolume = int.Parse(parameters["muleVolume"]);
                    muleClient.MinimumCoinsOnAccount = int.Parse(parameters["minimumCoinsOnAccount"]);
                    MuleManager.AddMuleClient(muleClient);

                    context.Response.Redirect("/mule");
                    context.Response.Close();
                }
            }
            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                var data = File.ReadAllText("Autobuyer.web/mule.html");
                var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
                if (nofiticationCount <= 0)
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", "");
                }
                else
                {
                    data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
                }

                data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());

                var stringBuilder = new StringBuilder();
                var clients = MuleManager.GetMuleClients();
                foreach(var client in clients)
                {
                    stringBuilder.AppendLine("<tr>");
                    var link = "<a target=\"_blank\" href=\"/mulesession?email=" + client.DestinationFUTAccount.EMail + "\">";

                    stringBuilder.AppendLine("<td>" + link + client.DestinationFUTAccount.EMail + "</td>");
                    stringBuilder.AppendLine("<td>" + client.CoinsOnDestinationAccount + "</td>");
                    stringBuilder.AppendLine("<td>" + client.MuleVolume + "</td>");
                    var lastMuling = client.GetLastMulingStatus();
                    if(lastMuling == null)
                    {
                        stringBuilder.AppendLine("<td></td>");
                        stringBuilder.AppendLine("<td></td>");
                    }
                    else
                    {
                        var printDate = lastMuling.DateTime.ToShortDateString() + " " + lastMuling.DateTime.ToLongTimeString();
                        stringBuilder.AppendLine("<td>" + printDate + "</td>");
                        stringBuilder.AppendLine("<td>" + lastMuling.Message + "</td>");
                    }
                    stringBuilder.AppendLine("<td><button class=\"btn btn-xs btn-success\" onclick=\"removeMuleSession('" + client.DestinationFUTAccount.EMail + "');\">Finished</button></td>");
                    stringBuilder.AppendLine("</tr>");

                    stringBuilder.AppendLine(Environment.NewLine);
                }

                data = data.Replace("{MULESTATISTIC}", stringBuilder.ToString());

                var platformID = int.Parse(ConfigurationManager.AppSettings["Platform"]);
                if (platformID == 0)
                {
                    data = data.Replace("<option value=\"1\">Playstation 3</option>", "");
                    data = data.Replace("<option value=\"2\">Xbox 360</option>", "");
                    data = data.Replace("<option value=\"3\">Xbox One</option>", "");
                }
                else if (platformID == 1)
                {
                    data = data.Replace("<option value=\"0\">Playstation 4</option>", "");
                    data = data.Replace("<option value=\"2\">Xbox 360</option>", "");
                    data = data.Replace("<option value=\"3\">Xbox One</option>", "");
                }
                else if (platformID == 2)
                {
                    data = data.Replace("<option value=\"0\">Playstation 4</option>", "");
                    data = data.Replace("<option value=\"1\">Playstation 3</option>", "");
                    data = data.Replace("<option value=\"3\">Xbox One</option>", "");
                }
                else if (platformID == 3)
                {
                    data = data.Replace("<option value=\"0\">Playstation 4</option>", "");
                    data = data.Replace("<option value=\"1\">Playstation 3</option>", "");
                    data = data.Replace("<option value=\"2\">Xbox 360</option>", "");
                }

                Write200Success(context, data, "text/html");
            }
        }
        private void HandleMuleSessionPage(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (!HasAccessToPage(context, WebAccessRole.Administrator))
            {
                context.Response.Redirect("/?access=no");
                context.Response.Close();
                return;
            }
            var data = File.ReadAllText("Autobuyer.web/mulesession.html"); ;
            var nofiticationCount = FUTLogsDatabase.GetFUTNotifications().Count;
            if (nofiticationCount <= 0)
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", "");
            }
            else
            {
                data = data.Replace("{NOTIFICATIONCOUNT}", nofiticationCount.ToString());
            }

            data = data.Replace("{DATABASENAME}", " - " + GetPlatformFromConfig());

            var parameters = HttpUtility.ParseQueryString(context.Request.Url.Query);
            var email = parameters["email"];
            var muleAccount = MuleManager.GetMuleClientByEMail(email);
            if (muleAccount != null)
            {
                data = data.Replace("{DESTINATIONACCOUNT}", muleAccount.DestinationFUTAccount.EMail);
                data = data.Replace("{MULEVOLUME}", muleAccount.MuleVolume.ToString());
                data = data.Replace("{MINIMUMCOINS}", muleAccount.MinimumCoinsOnAccount.ToString());
                if (!muleAccount.Muling && muleAccount.DestinationFUTAccount != null)
                {
                    data = data.Replace("name=\"startMule\" disabled=\"true\"", "name=\"startMule\"");
                }
                if (muleAccount.Muling)
                {
                    data = data.Replace("name=\"stopMule\" disabled=\"true\"", "name=\"stopMule\"");
                    
                }
                data = data.Replace("//removeForCallingInterval", "");
            }
            else
            {
                context.Response.Redirect("/mule");
                context.Response.Close();
                return;
            }

            Write200Success(context, data, "text/html");

        }
        private void GetMuleStatusRequest(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/login?redirectUrl=" + context.Request.Url.ToString());
                context.Response.Close();
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var responseText = "";

                var parameters = HttpUtility.ParseQueryString(context.Request.Url.Query);
                var email = parameters["email"];

                var muleAccount = MuleManager.GetMuleClientByEMail(email);
                if(muleAccount != null)
                {
                    if (muleAccount.GetMulingStatus() != null)
                    {
                        var mulestatus = muleAccount.GetMulingStatus();
                        var mulestatussorted = mulestatus.OrderBy(o => o.DateTime).ToList();
                        mulestatussorted.Reverse();
                        foreach (var status in mulestatussorted)
                        {
                            responseText += "<tr>";
                            responseText += "<td>" + status.EMail + "</td>";
                            var printDate = status.DateTime.ToShortDateString() + " " + status.DateTime.ToLongTimeString();
                            responseText += "<td>" + printDate + "</td>";
                            responseText += "<td>" + status.Message + "</td>";
                            responseText += "</tr>";
                        }
                    }
                }

                //if (MuleManager.GetMulingStatus() != null)
                //{
                //    var mulestatus = MuleManager.GetMulingStatus();
                //    var mulestatussorted = mulestatus.OrderBy(o => o.DateTime).ToList();
                //    mulestatussorted.Reverse();
                //    foreach (var status in mulestatussorted)
                //    {
                //        responseText += "<tr>";
                //        responseText += "<td>" + status.EMail + "</td>";
                //        var printDate = status.DateTime.ToShortDateString() + " " + status.DateTime.ToLongTimeString();
                //        responseText += "<td>" + printDate + "</td>";
                //        responseText += "<td>" + status.Message + "</td>";
                //        responseText += "</tr>";
                //    }
                //}
                Write200Success(context, responseText, "text/html");
            }
        }
        private void RemoveMuleSession(HttpListenerContext context)
        {
            if (!VerifySession(context))
            {
                context.Response.Redirect("/");
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                Write200Success(context, "", "text/html");
                return;
            }
            if (context.Request.HttpMethod == HttpMethod.Post.Method)
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var email = parameters["email"];

                var client = MuleManager.GetMuleClientByEMail(email);
                if(client != null)
                {
                    if(!client.Muling)
                    {
                        MuleManager.RemoveMuleClient(email);
                    }
                }
                context.Response.Redirect("/mule");
                context.Response.Close();
                return;
            }
        }
        #endregion

        private void Write200Success(HttpListenerContext context, string data, string mime = "")
        {
            if(mime != "")
            {
                context.Response.ContentType = mime;
            }
            var bytes = Encoding.UTF8.GetBytes(data);
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
            context.Response.KeepAlive = false;
            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        private void Write200Success(HttpListenerContext context, byte[] data, string mime = "")
        {
            if (mime != "")
            {
                context.Response.ContentType = mime;
            }
            context.Response.ContentLength64 = data.Length;
            context.Response.OutputStream.Write(data, 0, data.Length);
            context.Response.OutputStream.Close();
            context.Response.KeepAlive = false;
            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        private void Write404NotFound(HttpListenerContext context)
        {
            var bytes = Encoding.UTF8.GetBytes("404 Not Found");
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
            context.Response.StatusCode = 404;
            context.Response.KeepAlive = false;
            context.Response.Close();
        }

        private void Write500ServerError(HttpListenerContext context, string data = "")
        {
            var bytes = Encoding.UTF8.GetBytes("500 Internal Server Error\r\n\r\n" + data);
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
            context.Response.StatusCode = 500;
            context.Response.KeepAlive = false;
            context.Response.Close();
        }

        private void HandleSpecialRequest(HttpListenerContext context)
        {
            var file = "Autobuyer.web" + context.Request.Url.AbsolutePath;

            try
            {
                var data = File.ReadAllBytes(file);
                var extension = Path.GetExtension(file).ToLower();
                var mime = MimeHelper.GetMimeType(extension);
                context.Response.ContentType = mime;
                Write200Success(context, data);
            }
            catch
            {
                Write404NotFound(context);
            }
        }

        private bool VerifySession(HttpListenerContext context)
        {
            var allSalts = WebSessionsDatabase.GetWebSessions();
            Cookie sessionID = null;
            WebSession account = null;
            foreach (var salt in allSalts)
            {
                if (context.Request.Cookies["X-AB-SID_" + salt.Salt] != null)
                {
                    sessionID = context.Request.Cookies["X-AB-SID_" + salt.Salt];
                    account = salt;
                }
            }
            if(sessionID == null || account == null)
            {
                return false;
            }
            var verified = WebSessionVerifier.VerifySessionID(sessionID.Value, account.Salt);
            return verified;
        }

        private bool HasAccessToPage(HttpListenerContext context, WebAccessRole minimumAccess)
        {
            var allSalts = WebSessionsDatabase.GetWebSessions();
            Cookie sessionID = null;
            WebSession account = null;
            foreach (var salt in allSalts)
            {
                if (context.Request.Cookies["X-AB-SID_" + salt.Salt] != null)
                {
                    sessionID = context.Request.Cookies["X-AB-SID_" + salt.Salt];
                    account = salt;
                }
            }
            if (sessionID == null || account == null)
            {
                return false;
            }
            var verified = WebSessionVerifier.VerifySessionID(sessionID.Value, account.Salt);
            if(!verified)
            {
                return false;
            }
            return account.Role >= minimumAccess;
        }
    }
}
