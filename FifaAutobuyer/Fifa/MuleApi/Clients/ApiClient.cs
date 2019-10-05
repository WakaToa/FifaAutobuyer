using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;

namespace FifaAutobuyer.Fifa.MuleApi.Clients
{
    public enum MuleApiType
    {
        GameTradeEasy = 1,
        MuleFactory = 2,
        WholeSale = 3
    }

    public enum MuleApiStatus
    {
        Bought = 1,
        Cancel = 2
    }
    public abstract class ApiClient
    {
        public static ApiClient Client { get; set; }

        protected List<MuleApiPlayer> PlayersInQueue = new List<MuleApiPlayer>();
        public bool Running { get; set; }
        public Task LogicTask { get; set; }

        public abstract Task<List<MuleApiPlayer>> GetApiPlayerAsync();
        public abstract Task<bool> UpdatePlayerStatusAsync(long transactionId, MuleApiStatus status);
        public abstract Task LogicRoutineAsync();

        public string SKU { get; set; }

        protected ApiClient(string sku)
        {
            SKU = sku;
        }
        public string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        public void StopLogicRoutine()
        {
            Running = false;
        }

        public void StartLogicRoutine()
        {
            Running = true;

            LogicTask = new Task(async () => await LogicRoutineAsync());
            LogicTask.Start();
        }

        public async Task RunMuleApiClientAsync(FUTClient client, MuleApiPlayer muleApiPlayer)
        {
            client.AddLog($"Starting {muleApiPlayer.MuleApiType} MuleApi!");
            //var running = client.LogicRunningReal;
            //var runningCounter = 0;
            //if (running)
            //{
            //    client.AddLog($"Stopping Logic for MuleApi {muleApiPlayer.MuleApiType} transaction!");
            //    BotManager.StopBot(client.FUTAccount.EMail);
            //    while (client.LogicRunningReal)
            //    {
            //        await Task.Delay(1000);

            //        if (runningCounter++ > 30)
            //        {
                        
            //            client.AddLog($"Couldn't stop account!");
            //            client.StartLogic();
            //            return;
            //        }
            //    }
            //}

            var mule = new MuleApiClient(client, muleApiPlayer);
            var result = await mule.MuleLogicAsync();
            if (result)
            {
                switch (muleApiPlayer.MuleApiType)
                {
                    case MuleApiType.GameTradeEasy:
                        FUTMuleApiStatistic.Instance.GTETotalCoinVolume += muleApiPlayer.MuleValue;
                        FUTMuleApiStatistic.Instance.GTETotalDollarVolume += muleApiPlayer.Revenue;
                        break;
                    case MuleApiType.MuleFactory:
                        FUTMuleApiStatistic.Instance.MFTotalCoinVolume += muleApiPlayer.MuleValue;
                        FUTMuleApiStatistic.Instance.MFTotalDollarVolume += muleApiPlayer.Revenue;
                        break;
                    case MuleApiType.WholeSale:
                        FUTMuleApiStatistic.Instance.WSTotalCoinVolume += muleApiPlayer.MuleValue;
                        FUTMuleApiStatistic.Instance.WSTotalDollarVolume += muleApiPlayer.Revenue;
                        break;
                }
                FUTMuleApiStatistic.Instance.SaveChanges();
                client.CoinsMuledToday += muleApiPlayer.MuleValue;
                await muleApiPlayer.MuleApiClient.UpdatePlayerStatusAsync(muleApiPlayer.TransactionId, MuleApiStatus.Bought);
                FUTLogsDatabase.AddFUTNotification(client.FUTAccount.EMail, $"Muling API sold {muleApiPlayer.MuleValue} coins for {muleApiPlayer.Revenue}$ to {muleApiPlayer.MuleApiType}");
                client.AddLog($"Muling API sold {muleApiPlayer.MuleValue} coins for {muleApiPlayer.Revenue}$ to {muleApiPlayer.MuleApiType}");
            }
            else
            {
                await muleApiPlayer.MuleApiClient.UpdatePlayerStatusAsync(muleApiPlayer.TransactionId, MuleApiStatus.Cancel);
                client.AddLog($"Muling API failed! {muleApiPlayer.MuleValue} coins for {muleApiPlayer.Revenue}$ to {muleApiPlayer.MuleApiType}");
            }
            client.Muling = false;
            client.MulingPausedUntil = DateTime.Now.AddMinutes(FUTSettings.Instance.MuleApiSellDelayPerAccount);
            //if (running)
            //{
            //    client.AddLog($"Restarting account!");
            //    client.StartLogic();
            //}
        }
    }
}
