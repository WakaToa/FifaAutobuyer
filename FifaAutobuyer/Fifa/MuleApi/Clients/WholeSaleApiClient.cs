using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.MuleApi.Responses;
using FifaAutobuyer.Fifa.Services;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.MuleApi.Clients
{
    class WholeSaleApiClient : ApiClient
    {
        public const string PlatformPS4 = "FFA18PS4";
        public const string PlatformXB1 = "FFA18XBO";
        public new static WholeSaleApiClient Client { get; set; } = new WholeSaleApiClient(FUTSettings.Instance.WholeSaleClientPlatform);

        public string WholeSaleApiKey => FUTSettings.Instance.WholeSaleApiKey;

        public override async Task<List<MuleApiPlayer>> GetApiPlayerAsync()
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"https://sugar.sh/market/api?pass={WholeSaleApiKey}&skus={FUTSettings.Instance.WholeSaleClientPlatform}");

                WholeSaleResponse jsonResponse = null;


                jsonResponse = JsonConvert.DeserializeObject<WholeSaleResponse>(response);

                if (jsonResponse.ret != 0)
                {
                    return new List<MuleApiPlayer>();
                }

                var ret = new List<MuleApiPlayer>();



                ret.AddRange(jsonResponse.data.Select(playerResponse => new MuleApiPlayer
                {
                    MuleApiType = MuleApiType.WholeSale,
                    AssetId = int.Parse(playerResponse.maskedDefId),
                    BuyNowPrice = playerResponse.buyNowPrice,
                    ItemId = long.Parse(playerResponse.itemId),
                    Name = "",
                    StartingBid = playerResponse.startingBid,
                    TradeId = long.Parse(playerResponse.tradeId),
                    MuleApiClient = this,
                    Revenue = Math.Round(playerResponse.billingPrice / 10000, 5),
                    LockEnd = DateTime.UtcNow.AddMinutes(5)
                }));

                return ret;
            }
            catch
            {
                await Task.Delay(5000);
                return new List<MuleApiPlayer>();
            }

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


        public WholeSaleApiClient(string sku) : base(sku)
        {
        }
    }
}
