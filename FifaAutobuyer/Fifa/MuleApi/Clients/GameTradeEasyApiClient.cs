using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.MuleApi.Responses;
using FifaAutobuyer.Fifa.Services;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.MuleApi.Clients
{
    public class GameTradeEasyApiClient : ApiClient
    {
        public const string PlatformPS4 = "FFA18PS4";
        public const string PlatformXB1 = "FFA18XBO";
        public new static GameTradeEasyApiClient Client { get; set; } = new GameTradeEasyApiClient(FUTSettings.Instance.GTEClientPlatform);

        public string PartnerID => FUTSettings.Instance.GTEPartnerID;
        public string PartnerApiKey => FUTSettings.Instance.GTEPartnerApiKey;

        public override async Task<List<MuleApiPlayer>> GetApiPlayerAsync()
        {
            var httpClient = new HttpClient();
            var timestamp = Helper.CreateTimestamp() / 1000;

            var sign = MD5Hash(PartnerID + PartnerApiKey + timestamp);
            var response = await httpClient.GetStringAsync($"http://api.autobox.igvault.com/ffa18/api/pop/id/{PartnerID}/ts/{timestamp}/sign/{sign}/sku/{SKU}");

            GameTradeEasyResponse jsonResponse = null;

            if (response.Contains("1 player popped."))
            {
                var json2 = JsonConvert.DeserializeObject<GameTradeEasyResponse2>(response);
                jsonResponse = new GameTradeEasyResponse
                {
                    player = new List<GameTradeEasyResponse.GameTradeEasyPlayerResponse> {json2.player},
                    price = json2.price
                };
            }
            else
            {
                jsonResponse = JsonConvert.DeserializeObject<GameTradeEasyResponse>(response);
            }
            if (jsonResponse.player == null)
            {
                return new List<MuleApiPlayer>();
            }

            var ret = new List<MuleApiPlayer>();

          

            ret.AddRange(jsonResponse.player.Select(gameTradeEasyPlayerResponse => new MuleApiPlayer
            {
                MuleApiType = MuleApiType.GameTradeEasy,
                AssetId = gameTradeEasyPlayerResponse.assetId,
                BuyNowPrice = gameTradeEasyPlayerResponse.buyNowPrice,
                ItemId = gameTradeEasyPlayerResponse.itemId,
                Name = gameTradeEasyPlayerResponse.name,
                StartingBid = gameTradeEasyPlayerResponse.startingBid,
                TradeId = long.Parse(gameTradeEasyPlayerResponse.tradeId),
                MuleApiClient = this,
                Revenue = jsonResponse.price / 1000 * gameTradeEasyPlayerResponse.buyNowPrice,
                LockEnd = DateTime.UtcNow.AddMinutes(2.5)
            }));

            return ret;
        }
  
        public override Task<bool> UpdatePlayerStatusAsync(long transactionId, MuleApiStatus status)
        {
            return Task.FromResult(true);
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


        public GameTradeEasyApiClient(string sku) : base(sku)
        {
        }
    }
}
