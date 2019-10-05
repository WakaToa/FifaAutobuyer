using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.Fifa.MuleApi
{
    public class MuleApiClient
    {
        public FUTClient MuleFUTClient { get; set; }
        public MuleApiPlayer MulePlayer { get; set; }
        public MuleApiClient(FUTClient futClient, MuleApiPlayer mulePlayer)
        {
            MuleFUTClient = futClient;
            MulePlayer = mulePlayer;
        }

        public async Task<bool> MuleLogicAsync()
        {
            var searchObject = new MulingSearchObject
            {
                AssetID = MulePlayer.AssetId,
                RevisionID = 0,
                Type = FUTSearchParameterType.Player,
                MaxBuyNow = MulePlayer.BuyNowPrice,
                MinBuyNow = MulePlayer.BuyNowPrice
            };
            for (var i = 0; i <= 6; i++)
            {
                var auction = await MuleFUTClient.SearchForItemByTradeID(searchObject, MulePlayer.TradeId);

                if (auction != null)
                {
                    var secLeft = MulePlayer.LockEnd.Subtract(MulePlayer.LockStart).TotalSeconds;
                    if (secLeft > 5)
                    {
                        var result = await MuleFUTClient.BuyTradeAsync(auction.tradeId, auction.buyNowPrice);
                        if (result.auctionInfo?.Count > 0)
                        {
                            var item = result.auctionInfo.FirstOrDefault();
                            await MuleFUTClient.DiscardItemAsync(item.itemData.id, true);
                            MulePlayer.MuleValue = MulePlayer.BuyNowPrice - item.itemData.discardValue;
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    
                }
            }

            return false;
        }
    }
}
