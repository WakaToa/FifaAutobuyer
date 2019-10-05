//using FifaAutobuyer.Database;
//using FifaAutobuyer.Fifa.Managers;
//using NamedPipeWrapper;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace FifaAutobuyer.ServerWebpanel
//{
//    public class WebpanelClient
//    {
//        private static NamedPipeClient<string> _client;
//        private static string _ip;
//        public static void Start()
//        {
//            _ip = new WebClient() { Proxy = null }.DownloadString("https://api.ipify.org/");
//            _client = new NamedPipeClient<string>("FifaServerPipe");
//            _client.AutoReconnect = true;
//            _client.ServerMessage += MessageReceived;
//            _client.Error += Error;
//            _client.Start();


//            var thr = new Task(async () =>
//            {
//                while (true)
//                {
//                    SendResponse();
//                    await Task.Delay(TimeSpan.FromSeconds(15));
//                }
//            });
//            thr.Start();

//        }
//        private static void SendResponse()
//        {
//            var resp = GetResponse();
//            var json = JsonConvert.SerializeObject(resp);
//            _client.PushMessage(json);
//        }

//        private static void Error(Exception ex)
//        {
//            Console.WriteLine(ex);
//        }

//        private static void MessageReceived(NamedPipeConnection<string, string> connection, string rawMessage)
//        {
//            if(rawMessage == "GET")
//            {
//                SendResponse();
//            }
//        }

//        private static ClientOverview GetResponse()
//        {
//            try
//            {
//                var response = new ClientOverview();
//                response.Accounts = BotManager.GetFutClients().Count;
//                response.AccountsRunning = BotManager.GetFutClients().Where(x => x.LogicRunningReal).Count();

//                var totalCoins = 0;
//                var coinsPerAccount = 0;
//                var accounts = FUTAccountsDatabase.GetFUTAccounts();
//                var coins = FUTLogsDatabase.GetFUTCoins();

//                foreach (var acc in accounts)
//                {
//                    var coinsFromAcc = coins.Where(x => x.EMail.ToLower() == acc.EMail.ToLower()).FirstOrDefault();
//                    if (coinsFromAcc != null)
//                    {
//                        totalCoins += coinsFromAcc.Coins;
//                    }
//                }
//                if (totalCoins > 0 && accounts.Count > 0)
//                {
//                    coinsPerAccount = totalCoins / accounts.Count;
//                }

//                response.AverageCoinsPerAccount = coinsPerAccount;
//                response.TotalCoins = totalCoins;

//                var allTPItems = BotManager.GetTradepileItems();
//                var tpValue = (int)(allTPItems.Sum(x => x.buyNowPrice) * 0.95);

//                response.TotalOverallValue = (tpValue + totalCoins);
//                response.TotalTradepileItems = allTPItems.Count;
//                response.TotalTradepileValue = tpValue;

//                response.Platform = Program.GetPlatformFromConfig();
//                response.Notifications = FUTLogsDatabase.GetFUTNotifications().Count;
//                response.ProfitLast24Hours = FUTLogsDatabase.GetFUTProfitLogsLast24Hours().Sum(x => x.Profit);

//                response.BotInstance = "http://" + _ip + ":" + ConfigurationManager.AppSettings["WebAppPort"];
//                response.BotStatistic = "http://" + _ip + ":" + ConfigurationManager.AppSettings["WebAppPort"] + "/botstatistic";
//                response.LastUpdate = DateTime.Now;
//                return response;
//            }
//            catch(Exception e)
//            {
//            }
//            return new ClientOverview();
//        }
//    }
//}
