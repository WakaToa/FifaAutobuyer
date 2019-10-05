using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using FifaAutobuyer.Fifa.Services;

namespace FifaAutobuyer.Fifa.MuleApi
{
    public class MuleClient
    {
        public FUTClient DestinationFUTClient { get; set; }
        public FUTAccount DestinationFUTAccount { get; set; }
        public FUTClient RandomFUTClient { get; set; }

        public int MuleVolume { get; set; }
        public int MinimumCoinsOnAccount { get; set; }
        public int CoinsMuled { get; set; }
        public int CoinsOnDestinationAccount { get; set; }

        public void AddMulingStatus(MuleStatus status)
        {
            lock (_muleStatusLock)
            {
                if (_muleStatus.Count >= 25)
                {
                    var mulestatussorted = _muleStatus.OrderBy(o => o.DateTime).ToList();
                    mulestatussorted.Reverse();
                    _muleStatus.Remove(mulestatussorted.Last());
                }
                _muleStatus.Add(status);
            }
        }
        public List<MuleStatus> GetMulingStatus()
        {
            lock (_muleStatusLock)
            {
                return _muleStatus;
            }
        }
        public MuleStatus GetLastMulingStatus()
        {
            lock (_muleStatusLock)
            {
                return _muleStatus.OrderBy(x => x.DateTime).LastOrDefault();
            }
        }

        public string GetLastMuleStatusMessageString => GetLastMulingStatus()?.Message;
        public string GetLastMuleStatusTimeString => $"{GetLastMulingStatus()?.DateTime:d/M/yyyy HH:mm:ss}";

        private object _muleStatusLock = new object();
        private List<MuleStatus> _muleStatus = new List<MuleStatus>();

        public bool Muling { get; set; }

        public MuleClient(FUTAccount destinationAccount)
        {
            DestinationFUTAccount = destinationAccount;
        }

        private Dictionary<long, PriceLimitsResponse> _priceRanges = new Dictionary<long, PriceLimitsResponse>();
        private async Task<Tuple<AuctionInfo, PriceLimitsResponse>> GetBestAuctionForMuling(List<AuctionInfo> auctions)
        {
            foreach (var auction in auctions)
            {
                if (!_priceRanges.ContainsKey(auction.itemData.resourceId))
                {
                    var assetID = ResourceIDManager.GetAssetID(auction.itemData.resourceId);
                    var revID = ResourceIDManager.GetRevID(auction.itemData.resourceId);
                    var defID = ResourceIDManager.AssetIDToDefinitionID(assetID, revID);

                    var range = await DestinationFUTClient.GetPriceLimitsForItemAsync(defID);
                    if (range.maxPrice > 0)
                    {
                        _priceRanges.Add(auction.itemData.resourceId, range);
                    }

                }
            }

            var bestResults = _priceRanges.OrderBy(x => x.Value.maxPrice);


            AuctionInfo bestAuction = null;
            PriceLimitsResponse bestResult = null;
            foreach (var result in bestResults)
            {
                var auction = auctions.Where(x => x.itemData.resourceId == result.Key).OrderBy(x => x.buyNowPrice).FirstOrDefault();
                if (auction == null)
                {
                    continue;
                }
                if (bestAuction == null)
                {
                    bestAuction = auction;
                    bestResult = result.Value;
                }
                if (auction.buyNowPrice < bestAuction.buyNowPrice)
                {
                    bestAuction = auction;
                    bestResult = result.Value;
                }
            }



            return new Tuple<AuctionInfo, PriceLimitsResponse>(bestAuction, bestResult);
        }

        public void StartMule()
        {
            _muleThread = new Thread(MuleLogic);
            _muleThread.IsBackground = true;
            _muleThread.Start();
        }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public void StopMule()
        {
            AddMulingStatus(new MuleStatus("", "Trying to stop muling..."));
            _stopMuling = true;
        }

        private Thread _muleThread;
        private bool _stopMuling = false;
        private int _coinsBuffer = 15000;

        private async void MuleLogic()
        {
            Muling = true;
            _priceRanges = new Dictionary<long, PriceLimitsResponse>();
            AddMulingStatus(new MuleStatus("", "Starting Muling!"));
            if (DestinationFUTAccount == null)
            {
                FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, DestinationFUTClient.FUTAccount.EMail + "No destination account!");
                AddMulingStatus(new MuleStatus("", "No destination account!"));
                Muling = false;
                return;
            }
            DestinationFUTClient = new FUTClient(DestinationFUTAccount);
            AddMulingStatus(new MuleStatus("", "Destination account: " + DestinationFUTAccount.EMail));
            var loggedIn = new Tuple<bool, LoginResponse>(false, null);
            var currentTry = 0;
            
            while(!loggedIn.Item1)
            {
                if(_stopMuling)
                {
                    FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, DestinationFUTClient.FUTAccount.EMail + " Muling manually stopped!");
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, " Muling manually stopped!"));
                    Muling = false;
                    return;
                }
                currentTry++;
                AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Loging in try " + currentTry + "..."));
                loggedIn = await DestinationFUTClient.LoginAsync();
                if (!loggedIn.Item1)
                {
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Can not login to destination account! Error: " + Enum.GetName(typeof(FUTErrorCode), loggedIn.Item2.Code)));
                }
                if (!await DestinationFUTClient.GetTradingStatusEnabledAsync())
                {
                    FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, DestinationFUTClient.FUTAccount.EMail + " Transfermarket is not enabled! Muling stopped!");
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, " Transfermarket is not enabled! Muling stopped!"));
                    Muling = false;
                    return;
                }
                if(currentTry == 3)
                {
                    FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, DestinationFUTClient.FUTAccount.EMail + " Login failed 3 times! Muling stopped!");
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, " Login failed 3 times! Muling stopped!"));
                    Muling = false;
                    return;
                }
            }


            CoinsOnDestinationAccount = await DestinationFUTClient.GetCreditsAsync();
            AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Destination account has " + CoinsOnDestinationAccount + " credits!"));
            if (CoinsOnDestinationAccount < 400)
            {
                FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Credits! Muling stopped! Muling stopped!");
                AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Credits! Muling stopped! Muling stopped!"));
                Muling = false;
                return;
            }


            AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + "! Starting Muling!"));


            while (CoinsOnDestinationAccount < _coinsBuffer) //12000
            {
                try
                {
                    if (_stopMuling)
                    {
                        FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, DestinationFUTClient.FUTAccount.EMail + " Muling manually stopped!");
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, " Muling manually stopped!"));
                        Muling = false;
                        return;
                    }
                    if(!DestinationFUTClient.LoggedIn)
                    {
                        AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Destination account kicked! Reloging..."));
                        var result = await DestinationFUTClient.LoginAsync();
                        if(result.Item1)
                        {
                            AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Reloging success!"));
                        }
                        else
                        {
                            AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Reloging failed! Code: " + result.Item2.Message));
                        }
                        
                    }
                    CoinsOnDestinationAccount = await DestinationFUTClient.GetCreditsAsync();
                    AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Destination account has " + CoinsOnDestinationAccount + " credits!"));
                    if (CoinsOnDestinationAccount >= _coinsBuffer)
                    {
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Credits! Stopping Gold muling!!"));
                        break;
                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Credits! Gold Transfer starting!"));
                    RandomFUTClient = BotManager.GetMostValueableFUTClient(MinimumCoinsOnAccount + _coinsBuffer);
                    if (RandomFUTClient == null)
                    {
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't get a random FUTClient!"));
                        continue;
                    }
                    AddMulingStatus(new MuleStatus("", "Random FUTClient -> " + RandomFUTClient.FUTAccount.EMail));
                    var randomFutClientCredits = await RandomFUTClient.GetCreditsAsync();
                    AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Credits: " + randomFutClientCredits));
                    if (randomFutClientCredits <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Credits Error!"));
                    }
                    if (randomFutClientCredits <= MinimumCoinsOnAccount + _coinsBuffer)
                    {
                        RandomFUTClient.Muling = false;
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Credits are too low! Changing account!"));
                        continue;
                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Deleting all sold auctions!"));
                    await DestinationFUTClient.DeleteAllSoldAuctions();
                    var running = RandomFUTClient.LogicRunningReal;
                    if (running)
                    {
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Logic is running! Stopping Logic first!"));
                        RandomFUTClient.StopLogic();
                        while (RandomFUTClient.LogicRunningReal)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Waiting for Logic to stop!"));
                            await Task.Delay(5000);
                        }

                    }

                    var mulingGoldPlayerSearchObject = new MulingSearchObject() { Level = "gold", MaxBuyNow = 400, Type = FUTSearchParameterType.Player };
                    var playerToBuy = await DestinationFUTClient.GetTransferMarketResponseAsync(mulingGoldPlayerSearchObject);
                    if (playerToBuy.auctionInfo == null || playerToBuy.auctionInfo.Count == 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't search for gold players!" + playerToBuy.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    var playerGoldTransfer = playerToBuy.auctionInfo.FirstOrDefault();

                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Trying to buy " + playerGoldTransfer.tradeId + " for " + playerGoldTransfer.buyNowPrice));

                    var boughtPlayer = await DestinationFUTClient.BuyTradeAsync(playerGoldTransfer.tradeId, playerGoldTransfer.buyNowPrice);
                    if (boughtPlayer.auctionInfo == null || boughtPlayer.auctionInfo.Count == 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't buy gold player!" + boughtPlayer.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Bought " + boughtPlayer.auctionInfo.FirstOrDefault().tradeId + " for " + boughtPlayer.auctionInfo.FirstOrDefault().buyNowPrice));

                    var boughtPlayerAssetID = ResourceIDManager.GetAssetID(boughtPlayer.auctionInfo.FirstOrDefault().itemData.resourceId);
                    var priceLimits = await DestinationFUTClient.GetPriceLimitsForItemAsync(boughtPlayerAssetID);
                    if (priceLimits.maxPrice <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't get price limits!"));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Moving item to tradepile!"));
                    var tradepileMoved = await DestinationFUTClient.MoveItemToTradepileAsync(boughtPlayer.auctionInfo.FirstOrDefault().itemData.id);
                    if (tradepileMoved.itemData == null || tradepileMoved.itemData.Count <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't move item to tradepile!" + tradepileMoved.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }

                    var mulingPrice = ((int)(((double)Helper.RandomInt(80, 90) / 100) * priceLimits.maxPrice)).ValidatePrice();


                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Offering " + tradepileMoved.itemData[0].id + " for " + mulingPrice));
                    var offerModel = new OfferItemModel(tradepileMoved.itemData[0].id, mulingPrice);
                    var randomBuy = new Random().Next(priceLimits.minPrice.IncrementPrice(), mulingPrice.DecrementPrice());
                    var uniqueBuy = randomBuy.ValidatePrice();
                    offerModel.StartingBid = uniqueBuy;
                    var goldAuctionPlaced = await DestinationFUTClient.OfferItemOnTransferMarketAsync(offerModel);
                    if (goldAuctionPlaced.id <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't offer item on transfermarket!" + goldAuctionPlaced.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }


                    await Task.Delay(10000);
                    var goldSearchObject = new MulingSearchObject();
                    goldSearchObject.AssetID = ResourceIDManager.GetAssetID(boughtPlayer.auctionInfo.FirstOrDefault().itemData.resourceId);
                    goldSearchObject.RevisionID = ResourceIDManager.GetRevID(boughtPlayer.auctionInfo.FirstOrDefault().itemData.resourceId);
                    goldSearchObject.Type = FUTSearchParameterType.Player;
                    goldSearchObject.MinBuyNow = mulingPrice;
                    goldSearchObject.MaxBid = uniqueBuy;
                    goldSearchObject.MinBid = uniqueBuy;

                    AuctionInfo goldObjectOnMarket = null;
                    var searchTries = 0;
                    while (true)
                    {
                        await Task.Delay(5000);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Searching for tradeID " + goldAuctionPlaced.id + " Try #" + searchTries));
                        goldObjectOnMarket = await RandomFUTClient.SearchForItemByTradeID(goldSearchObject, goldAuctionPlaced.id);
                        if (goldObjectOnMarket != null)
                        {
                            break;
                        }
                        if (searchTries >= 3)
                        {
                            break;
                        }
                        searchTries++;
                    }


                    if (goldObjectOnMarket == null)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Couldn't find gold player on market!"));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Found card!"));
                    AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Trying to buy auction!"));
                    var boughtAuction = await RandomFUTClient.BuyTradeAsync(goldObjectOnMarket.tradeId, goldObjectOnMarket.buyNowPrice);
                    if (boughtAuction.auctionInfo == null)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Couldn't buy gold player on market!" + boughtAuction.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    else
                    {
                        RandomFUTClient.Muling = false;
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Successfully bought player!"));
                        await RandomFUTClient.DiscardItemAsync(boughtAuction.auctionInfo[0].itemData.id);
                        //discard
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                }
                catch (Exception e)
                {
                    if (RandomFUTClient != null)
                    {
                        RandomFUTClient.Muling = false;
                    }
                    AddMulingStatus(new MuleStatus("", "Exception:\r\n" + e));
                }

            }




            AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Credits! Muling starting!"));
            AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Destination account coins after mule => " + MuleVolume));

            //Muling
            while (CoinsOnDestinationAccount < MuleVolume)
            {
                try
                {
                    if (_stopMuling)
                    {
                        FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, DestinationFUTClient.FUTAccount.EMail + " Muling manually stopped!");
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, " Muling manually stopped!"));
                        Muling = false;
                        return;
                    }
                    CoinsOnDestinationAccount = await DestinationFUTClient.GetCreditsAsync();
                    AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Destination account has " + CoinsOnDestinationAccount + " credits!"));
                    AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Coins muled: " + CoinsMuled));
                    if (CoinsOnDestinationAccount >= MuleVolume)
                    {
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Credits! Stopping muling!!"));
                        break;
                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Credits! Muling starting!"));
                    RandomFUTClient = BotManager.GetMostValueableFUTClient(MinimumCoinsOnAccount + _coinsBuffer);
                    if (RandomFUTClient == null)
                    {
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't get a random FUTClient!"));
                        continue;
                    }
                    AddMulingStatus(new MuleStatus("", "Random FUTClient -> " + RandomFUTClient.FUTAccount.EMail));
                    var randomFutClientCredits = await RandomFUTClient.GetCreditsAsync();
                    AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Credits: " + randomFutClientCredits));
                    if (randomFutClientCredits <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Credits Error!"));
                    }

                    if (randomFutClientCredits < MinimumCoinsOnAccount + _coinsBuffer) // + 60000
                    {
                        RandomFUTClient.Muling = false;
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Credits are too low (predicted)! Changing account!"));
                        continue;
                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTAccount.EMail, "Deleting all sold auctions!"));
                    await DestinationFUTClient.DeleteAllSoldAuctions();
                    var running = RandomFUTClient.LogicRunningReal;
                    if (running)
                    {
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Logic is running! Stopping Logic first!"));
                        RandomFUTClient.StopLogic();
                        while (RandomFUTClient.LogicRunningReal)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Waiting for Logic to stop!"));
                            await Task.Delay(5000);
                        }

                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Searching for gold if player!"));
                    var mulingGoldIFPlayerSearchObject = new MulingSearchObject() { Level = "gold", MinBuyNow = 200, MaxBuyNow = 15000, Type = FUTSearchParameterType.Player, Rare = "SP" };

                    var goldIFAuctions = await DestinationFUTClient.SearchPagesAsync(mulingGoldIFPlayerSearchObject, 2);

                    if (goldIFAuctions == null || goldIFAuctions.Count == 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't search for gold if players!"));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }

                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Trying to calculate best player from " + goldIFAuctions.Count + " auctions!"));
                    var bestPlayer = await GetBestAuctionForMuling(goldIFAuctions);
                    if (bestPlayer.Item1 == null || bestPlayer.Item2 == null || bestPlayer.Item2.maxPrice <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't definde best auction for muling!"));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    var coinsToTransferOld = bestPlayer.Item2.maxPrice;

                    var randomPercent = (double)Helper.RandomInt(80, 90)/100;
                    var coinsToTransfer = ((int)(randomPercent*bestPlayer.Item2.maxPrice)).ValidatePrice();
                    if (coinsToTransfer > randomFutClientCredits - MinimumCoinsOnAccount)
                    {
                        coinsToTransfer = (randomFutClientCredits - MinimumCoinsOnAccount).ValidatePrice();
                    }

                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Best Player value: " + bestPlayer.Item2.maxPrice));
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Muling: " + coinsToTransfer));
                    //if (randomFutClientCredits <= MinimumCoinsOnAccount + bestPlayer.Item2.maxPrice)
                    //{
                    //    AddMulingStatus(new MuleStatus(randomFutClient.FUTAccount.EMail, "Credits are too low! Changing account!"));
                    //    continue;
                    //}

                    var boughtPlayer = await DestinationFUTClient.BuyTradeAsync(bestPlayer.Item1.tradeId, bestPlayer.Item1.buyNowPrice);
                    if (boughtPlayer.auctionInfo == null || boughtPlayer.auctionInfo.Count == 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't buy gold if player!" + boughtPlayer.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Bought " + boughtPlayer.auctionInfo.FirstOrDefault().tradeId + " for " + boughtPlayer.auctionInfo.FirstOrDefault().buyNowPrice));


                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Moving item to tradepile!"));
                    var tradepileMoved = await DestinationFUTClient.MoveItemToTradepileAsync(boughtPlayer.auctionInfo.FirstOrDefault().itemData.id);
                    if (tradepileMoved.itemData == null || tradepileMoved.itemData.Count <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't move item to tradepile!" + tradepileMoved.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }



                    AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Offering " + tradepileMoved.itemData[0].id + " for " + coinsToTransfer));

                    var offerModel = new OfferItemModel(tradepileMoved.itemData[0].id, coinsToTransfer);
                    var randomBuy = new Random().Next(bestPlayer.Item2.minPrice.IncrementPrice(), coinsToTransfer.DecrementPrice());
                    var uniqueBuy = randomBuy.ValidatePrice();
                    offerModel.StartingBid = uniqueBuy;
                    var goldAuctionPlaced = await DestinationFUTClient.OfferItemOnTransferMarketAsync(offerModel);
                    if (goldAuctionPlaced.id <= 0)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Couldn't offer item on transfermarket!" + goldAuctionPlaced.GetError()));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }

                    await Task.Delay(10000);
                    var goldSearchObject = new MulingSearchObject();
                    goldSearchObject.AssetID = ResourceIDManager.GetAssetID(boughtPlayer.auctionInfo.FirstOrDefault().itemData.resourceId);
                    goldSearchObject.RevisionID = ResourceIDManager.GetRevID(boughtPlayer.auctionInfo.FirstOrDefault().itemData.resourceId);
                    goldSearchObject.Type = FUTSearchParameterType.Player;
                    goldSearchObject.MinBuyNow = coinsToTransfer;
                    goldSearchObject.MaxBid = uniqueBuy;
                    goldSearchObject.MinBid = uniqueBuy;

                    AuctionInfo goldObjectOnMarket = null;
                    var searchTries = 0;
                    while (true)
                    {
                        await Task.Delay(10000);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Searching for tradeID " + goldAuctionPlaced.id + " Try #" + searchTries));
                        goldObjectOnMarket = await RandomFUTClient.SearchForItemByTradeID(goldSearchObject, goldAuctionPlaced.id);
                        if (goldObjectOnMarket != null)
                        {
                            break;
                        }
                        if (searchTries >= 10)
                        {
                            break;
                        }
                        searchTries++;
                    }


                    if (goldObjectOnMarket == null)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Couldn't find bought gold if player on market!"));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Found card!"));
                    AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Trying to buy auction!"));
                    BuyItemResponse boughtAuction = null;
                    var buyTries = 0;
                    while (true)
                    {
                        await Task.Delay(10000);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Trying to buy tradeID " + goldObjectOnMarket.tradeId + " Try #" + buyTries));
                        boughtAuction = await RandomFUTClient.BuyTradeAsync(goldObjectOnMarket.tradeId, goldObjectOnMarket.buyNowPrice);
                        if (boughtAuction != null && boughtAuction.auctionInfo != null)
                        {
                            break;
                        }
                        else
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Buying failed! Errors: " + boughtAuction.Code + " " + boughtAuction.Message + " " + boughtAuction.Reason + " " + boughtAuction.String + " " + boughtAuction.Debug));
                        }
                        if (buyTries >= 10)
                        {
                            break;
                        }
                        buyTries++;
                    }

                    if (boughtAuction == null || boughtAuction.auctionInfo == null)
                    {
                        RandomFUTClient.Muling = false;
                        RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Couldn't find gold if player on market!"));
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                    else
                    {
                        CoinsMuled += (int)(boughtAuction.auctionInfo[0].buyNowPrice * 0.95);
                        AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, "Successfully bought player!"));
                        //place for minbin or discard

                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Moving item to tradepile!"));
                        var tradepileMovedNew = await RandomFUTClient.MoveItemToTradepileAsync(boughtAuction.auctionInfo.FirstOrDefault().itemData.id);
                        if (tradepileMovedNew.itemData == null || tradepileMovedNew.itemData.Count <= 0)
                        {
                            RandomFUTClient.Muling = false;
                            RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Couldn't move item to tradepile!" + tradepileMovedNew.GetError()));
                            if (running)
                            {
                                AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                                RandomFUTClient.StartLogic();
                            }
                            continue;
                        }
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Offering item for minBuy!"));
                        var offeredItemNew = await RandomFUTClient.OfferItemOnTransferMarketAsync(new OfferItemModel(tradepileMovedNew.itemData[0].id, bestPlayer.Item2.minPrice.IncrementPrice()));
                        if (offeredItemNew.id <= 0)
                        {
                            RandomFUTClient.Muling = false;
                            RandomFUTClient.MulingPausedUntil = DateTime.Now.AddMinutes(30);
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Couldn't offer item for min price on transfermarket! Trying to discard item!"));
                            var result = await RandomFUTClient.DiscardItemAsync(tradepileMovedNew.itemData[0].id);
                            if (running)
                            {
                                AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                                RandomFUTClient.StartLogic();
                            }
                            continue;
                        }
                        AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Muling success!"));
                        RandomFUTClient.Muling = false;
                        if (running)
                        {
                            AddMulingStatus(new MuleStatus(RandomFUTClient.FUTAccount.EMail, "Restarting Logic!"));
                            RandomFUTClient.StartLogic();
                        }
                        continue;
                    }
                }
                catch (Exception e)
                {
                    if(RandomFUTClient != null)
                    {
                        RandomFUTClient.Muling = false;
                    }
                    AddMulingStatus(new MuleStatus("", "Exception:\r\n" + e));
                }

            }



            FUTLogsDatabase.AddFUTNotification("Muling " + DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Muling finished!");
            AddMulingStatus(new MuleStatus(DestinationFUTClient.FUTAccount.EMail, CoinsOnDestinationAccount + " Muling finished!"));
            Muling = false;
            return;
        }
    }
}
