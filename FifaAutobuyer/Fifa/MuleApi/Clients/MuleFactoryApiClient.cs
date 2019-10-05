using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.MuleApi.Requests;
using FifaAutobuyer.Fifa.MuleApi.Responses;
using FifaAutobuyer.Fifa.Services;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.MuleApi.Clients
{
    public class MuleFactoryApiClient : ApiClient
    {
        public const string PlatformPS4 = "ps4";
        public const string PlatformXB1 = "xbox";
        public new static MuleFactoryApiClient Client { get; set; } = new MuleFactoryApiClient(FUTSettings.Instance.MuleFactoryClientPlatform);

        public string User => FUTSettings.Instance.MuleFactoryUser;
        public string SecretWord => FUTSettings.Instance.MuleFactorySecretWord;
        public int MaximumBuyOutPrice => FUTSettings.Instance.MuleApiMaxTransactionValue;

        public override async Task<List<MuleApiPlayer>> GetApiPlayerAsync()
        {
            var httpClient = new HttpClient();
            var timestamp = Helper.CreateTimestamp();

            var request = new MuleFactoryGetPlayerRequest();
            request.timestamp = timestamp;
            request.maximumBuyOutPrice = MaximumBuyOutPrice;
            request.user = User;
            request.platform = SKU;
            request.hash = MD5Hash($"{User}{SKU}{MaximumBuyOutPrice}{timestamp}{SecretWord}");
            var response = await httpClient.PostAsync("https://cdss.machineword.com/trade_interface/request/", new StringContent(JsonConvert.SerializeObject(request)));
            var responseString = await response.Content.ReadAsStringAsync();

            var json = JsonConvert.DeserializeObject<MuleFactoryGetPlayerResponse>(responseString);
            if (json.code != 200)
            {
                return new List<MuleApiPlayer>();
            }

           return new List<MuleApiPlayer>(){ new MuleApiPlayer(){LockEnd = (((long)json.lockExpires) * 1000).ToDateTime(), AssetId = json.assetID, MuleApiClient = this, BuyNowPrice = json.coinAmount, ItemId = 0, TradeId = json.tradeID, MuleApiType = MuleApiType.MuleFactory, Revenue = json.paymentInUsd, TransactionId = json.transactionID, Name = "", StartingBid = json.startPrice}};
        }

        public override async Task<bool> UpdatePlayerStatusAsync(long transactionId, MuleApiStatus st)
        {
            var httpClient = new HttpClient();
            var ts = Helper.CreateTimestamp();

            var result = (st == MuleApiStatus.Bought ? "bought" : "cancel");
            var request = new MuleFactoryUpdateStatusRequest
            {
                transactionID = transactionId,
                timestamp = ts,
                user = User,
                status = result,
                hash = MD5Hash($"{User}{transactionId}{result}{ts}{SecretWord}")
            };

            var response = await httpClient.PostAsync("https://cdss.machineword.com/trade_interface/status/", new StringContent(JsonConvert.SerializeObject(request)));
            var responseString = await response.Content.ReadAsStringAsync();

            return result == responseString;
        }

        public override async Task LogicRoutineAsync()
        {
            while (Running)
            {
                var futClient = BotManager.GetMostValueableFUTClient(FUTSettings.Instance.MuleApiMaxTransactionValue + FUTSettings.Instance.MuleApiMinCoinsOnAccount);
                if (futClient != null)
                {
                    var startedMule = false;
                    var players = await GetApiPlayerAsync();
                    players.RemoveAll(x => x.BuyNowPrice > FUTSettings.Instance.MuleApiMaxTransactionValue);
                    foreach (var muleApiPlayer in players)
                    {
                        futClient.Muling = true;
                        muleApiPlayer.LockStart = DateTime.UtcNow;
                        Task.Factory.StartNew(() => RunMuleApiClientAsync(futClient, muleApiPlayer));
                        startedMule = true;
                        break;
                    }
                    if (!startedMule)
                    {
                        futClient.Muling = false;
                    }

                }
                await Task.Delay(FUTSettings.Instance.MuleApiRequestDelay);
            }
        }

        public MuleFactoryApiClient(string sku) : base(sku)
        {
        }
    }
}
