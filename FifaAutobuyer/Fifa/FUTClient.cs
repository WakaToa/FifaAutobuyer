using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Collection;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Factories;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Requests;
using FifaAutobuyer.Fifa.Responses;
using FifaAutobuyer.Fifa.Semaphore;
using FifaAutobuyer.Fifa.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.ActionScheduler;
using FifaAutobuyer.Fifa.Captcha;
using FifaAutobuyer.Fifa.PriceCalculation;

namespace FifaAutobuyer.Fifa
{
    public class FUTClient
    {
        public FUTAccount FUTAccount { get; set; }
        public FUTProxy FUTProxy { get; set; }

        public bool LogicRunning { get; set; }
        public bool LogicRunningReal { get; set; }

        private Thread _logicThread;

        private FUTRequestFactory _futRequestFactory;
        public FUTClient(FUTAccount account)
        {
            LoggedIn = false;
            FUTAccount = account;
            FUTAccountStatistic = new FUTAccountStatistic();
            FUTAccountStatistic.Load(FUTAccount.EMail);
            TradepileItems = new List<AuctionInfo>();
            var coins = FUTLogsDatabase.GetFUTCoinsByEMail(account.EMail);
            if (coins != null)
            {
                Coins = coins.Coins;
            }

            LoginMethod = FUTSettings.Instance.LoginMethod;
            
            AllocateProxy();
        }

        public void AllocateProxy()
        {
            FUTProxy = ProxyManager.GetFUTProxy(FUTAccount.GetFUTProxy());
            FUTAccount.FUTProxyID = FUTProxy?.ID ?? -1;
            FUTAccount.SaveChanges();
            ResetRequestFactory();
        }
        public void DeallocateProxy()
        {
            FUTProxy = null;
            FUTAccount.FUTProxyID = -1;
            FUTAccount.SaveChanges();
            ResetRequestFactory();
        }
        public void UpdateProxy(FUTProxy proxy)
        {
            if (!string.IsNullOrEmpty(proxy?.Host))
            {
                _futRequestFactory.SetProxy($"https://{proxy.ToString()}", proxy.Username, proxy.Password);
            }
            else
            {
                _futRequestFactory.SetProxy("", "", "");
            }
        }

        private void ResetRequestFactory()
        {
            _futRequestFactory = new FUTRequestFactory(FUTAccount);
            _futRequestFactory.CurrentLoginMethod = LoginMethod;
            UpdateProxy(FUTProxy);
            LoggedIn = false;
        }

        public FUTAccountStatistic FUTAccountStatistic { get; set; }

        public int CoinsMuledToday { get; set; }
        public int Coins { get; set; }
        public bool Muling { get; set; }
        public DateTime? MulingPausedUntil { get; set; }
        public bool LoggedIn { get; set; }
        public bool LogicPaused { get; set; }
        public bool PriceChecking { get; set; }

        private DateTime _lastKeepAlive = DateTime.Now;

        public void StartLogic()
        {
            if (!LoggedIn)
            {
                ResetRequestFactory();
            }
            
            
            if (_logicThread != null)
            {
                _logicThread = null;
            }
            LogicRunning = true;
            _logicThread = new Thread(LogicRoutine);
            _logicThread.Name = FUTAccount.EMail + "_LogicThread";
            _logicThread.Priority = ThreadPriority.Highest;
            _logicThread.IsBackground = true;
            _logicThread.Start();
            
            AddLog("Logic started!");

            UpdateStatistic("Logic started!");
        }
        public void StopLogic()
        {
            LogicRunning = false;
            AddLog("Logic Stop Requested!");
            UpdateStatistic("Logic Stop Requested!");
        }

        private DateTime? _pausedLogicUntil = null;
        public void PauseLogic(double hours)
        {
            hours = Math.Round(hours, 2);
            AddLog("Pausing Logic for " + hours + " hours!");
            UpdateStatistic("Pausing Logic for " + hours + " hours!");
            _pausedLogicUntil = DateTime.Now.AddHours(hours);
            FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Pausing logic until ~" + ((DateTime)_pausedLogicUntil).ToLongTimeString() + " " +  ((DateTime)_pausedLogicUntil).ToLongDateString() + "!");
            LogicPaused = true;
        }
        public LoginMethod LoginMethod { get; set; }

        private int _isBuyRoutineCounter = 0;

        private DateTime _maxCardsPause = DateTime.Now.Subtract(new TimeSpan(24, 0, 0));

       

        private async void LogicRoutine()
        {
            try
            {
                FUTListItem pricecheckItem = null;
                while (LogicRunning)
                {
                    try
                    {
                        LogicRunningReal = true;
                        
                        #region Paused Logic
                        if (_pausedLogicUntil != null)
                        {
                            if (_pausedLogicUntil >= DateTime.Now)
                            {
                                var realDT = (DateTime)_pausedLogicUntil;
                                UpdateStatistic("Logic still paused until ~" + realDT.ToLongTimeString() + realDT.ToLongDateString() + "!");
                                LogicPaused = true;
                                await Task.Delay(10000);
                                continue;
                            }
                            else
                            {
                                FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Logic pausing finished! Continuing...");
                                AddLog("Logic pausing finished! Continuing...");
                                UpdateStatistic("Logic pausing finished! Continuing...");
                                _pausedLogicUntil = null;
                                LogicPaused = false;
                                continue;
                            }
                        }
                        #endregion


                        #region LoginSwitch
                        if (LogicRunning)
                        {
                            var actualLoginMethod = FUTSettings.Instance.LoginMethod;
                            if (actualLoginMethod != LoginMethod)
                            {
                                //FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "LoginMethod changed! Resetting Factor and Relogging!");
                                LoginMethod = actualLoginMethod;
                                LoggedIn = false;
                                ResetRequestFactory();
                                continue;
                            }
                        }
                        #endregion
                        #region LoginRoutine
                        if (!LoggedIn && LogicRunning)
                        {
                            var resp = await LoginAsync();
                            if (resp.Item2.Code == FUTErrorCode.WrongLoginData)
                            {
                                AddLog("Logic stopped!");
                                UpdateStatistic("Logic stopped!");
                                LogicRunningReal = false;
                                break;
                            }
                            if (!resp.Item1)
                            {
                                await Task.Delay(5000);
                                continue;
                            }
                        }
                        #endregion

                        #region Keep-Alive
                        if (DateTime.Now.Subtract(_lastKeepAlive).TotalMinutes >= 10)
                        {
                            await RenewCoinsAsync();
                            _lastKeepAlive = DateTime.Now;
                        }
                        #endregion

                        if (Coins <= 0 && LogicRunning)
                        {
                            await RenewCoinsAsync();
                        }

                        #region DiscardeverythingRoutine
                        if (FUTSettings.Instance.DiscardEverything && LogicRunning)
                        {
                            await DiscardEverything();
                            await Task.Delay(10000);
                            continue;
                        }
                        #endregion

                        #region PriceCheckRoutine
                        if (LoggedIn && _futRequestFactory.RequestPerMinuteManagerSearch.IsNextRequestPossible() && LogicRunning)
                        {
                            pricecheckItem = ItemListManager.GetNextPriceCheckItem();
                            if (pricecheckItem != null)
                            {
                                PriceChecking = true;
                                AddLog("Starting pricecheck for " + pricecheckItem.DisplayName);
                                UpdateStatistic("Starting pricecheck for " + pricecheckItem.DisplayName);


                                var avgPrice = 0;
                                if (pricecheckItem.Type == FUTSearchParameterType.Player)
                                {
                                    avgPrice = await PriceCheckPlayerFast(pricecheckItem); //await PriceCheckPlayerCalculation(pricecheckItem);
                                }
                                else
                                {
                                    avgPrice = await PriceCheckAsyncAverage(pricecheckItem);
                                }

                                if (avgPrice > 0)
                                {
                                    var lastPricesDB =
                                        FUTLogsDatabase.GetFUTPriceChecksByAssetIDRevisionID(pricecheckItem.AssetID,
                                            pricecheckItem.RevisionID, FUTSettings.Instance.UseLastPriceChecks);
                                    var lastPrices = lastPricesDB.Sum(x => x.AveragePrice);

                                    var dealer = FUTSettings.Instance.UseLastPriceChecks;
                                    //if (pricecheckItem.Type == FUTSearchParameterType.Player)
                                    //{
                                    //    dealer = SecondAverageTable.GetRange(avgPrice, pricecheckItem.Rating);
                                    //}
                                    if (dealer > lastPricesDB.Count)
                                    {
                                        dealer = lastPricesDB.Count;
                                    }
                                    var avgPriceNew = 0;
                                    if (dealer > 0)
                                    {
                                        var oldAVG = lastPrices / dealer;
                                        var bigger = Math.Max(oldAVG, avgPrice);
                                        var smaller = Math.Min(oldAVG, avgPrice);
                                        var percentage = (int)((((float)bigger / (float)smaller) - (smaller / smaller)) * 100);
                                        if (percentage >= FUTSettings.Instance.PriceCorrectionPercentage &&
                                            FUTSettings.Instance.PriceCorrectionPercentage > 0)
                                        {
                                            AddLog("Pricecheck failed for " + pricecheckItem.DisplayName + " because new price(" +
                                                   avgPrice + ") is " + percentage + "% bigger than old price(" + +oldAVG +
                                                   ").");
                                            UpdateStatistic("Pricecheck failed for " + pricecheckItem.DisplayName +
                                                            " because new price(" + avgPrice + ") is " + percentage +
                                                            "% bigger than old price(" + +oldAVG + ").");
                                            pricecheckItem.LastPriceCheck = Helper.CreateTimestamp();
                                            pricecheckItem.PriceChecking = false;
                                            pricecheckItem.SaveChanges();
                                            continue;
                                        }
                                        var allPrices = lastPrices + avgPrice;
                                        avgPriceNew = (allPrices / (dealer + 1)).ValidatePrice();
                                    }
                                    else
                                    {
                                        avgPriceNew = avgPrice;
                                    }

                                    var priceString = Environment.NewLine + "Last Prices: ";
                                    foreach (var price in lastPricesDB)
                                    {
                                        priceString = priceString + price.AveragePrice + Environment.NewLine;
                                    }
                                    priceString = priceString + avgPrice + Environment.NewLine;
                                    priceString += "Average: " + avgPriceNew;

                                    var oldBuy = pricecheckItem.BuyPrice;
                                    var oldSell = pricecheckItem.SellPrice;

                                    var buyNow = avgPriceNew / 100 * pricecheckItem.VariableBuyPercent;
                                    buyNow = buyNow.ValidatePrice();

                                    var sellPrice = avgPriceNew / 100 * pricecheckItem.SellPercent;
                                    sellPrice = sellPrice.ValidatePrice();



                                    pricecheckItem.BuyPrice = buyNow;
                                    pricecheckItem.SellPrice = sellPrice;

                                    AddLog("Pricecheck finished for " + pricecheckItem.DisplayName + Environment.NewLine +
                                           "Old Values: " + oldBuy + "|" + oldSell + Environment.NewLine + "New Values: " +
                                           buyNow + "|" + sellPrice + priceString);
                                    UpdateStatistic("Pricecheck finished for " + pricecheckItem.DisplayName + " " +
                                                    "Old Values: " + oldBuy + "|" + oldSell + " " + "New Values: " + buyNow +
                                                    "|" + sellPrice);

                                    var futPriceCheck = new FUTPriceCheck(pricecheckItem.AssetID, pricecheckItem.RevisionID,
                                        avgPrice, buyNow, sellPrice, pricecheckItem.VariableBuyPercent,
                                        pricecheckItem.SellPercent);
                                    FUTLogsDatabase.InsertFUTPriceCheck(futPriceCheck);


                                }
                                else
                                {
                                    AddLog("Pricecheck finished for " + pricecheckItem.DisplayName +
                                           " Couldn't calculate average price! Setting BuyPrice to 0!");
                                    UpdateStatistic("Pricecheck finished for " + pricecheckItem.DisplayName +
                                                    " Couldn't calculate average price! Setting BuyPrice to 0!");
                                    pricecheckItem.BuyPrice = 0;
                                }

                                PriceChecking = false;
                                pricecheckItem.LastPriceCheck = Helper.CreateTimestamp();
                                pricecheckItem.PriceChecking = false;
                                pricecheckItem.SaveChanges();
                                continue;
                            }
                        }
                        #endregion

                        #region TradepileRoutine
                        if (_lastTradepileRoutine == null || DateTime.Now.Subtract(_lastTradepileRoutine).TotalSeconds >= FUTSettings.Instance.TradepileCheck && FUTSettings.Instance.EnableSell && _futRequestFactory.RequestPerMinuteManagerSearch.IsNextRequestPossible() && LogicRunning)
                        {
                            await TradepileRoutine().ConfigureAwait(false);

                            _lastTradepileRoutine = DateTime.Now;
                        }


                        var isTradepileFull = false;
                        if (TradepileItems != null && LogicRunning && FUTSettings.Instance.EnableSell)
                        {
                            if (_tradepileSize == -1)
                            {
                                await TradepileRoutine();
                            }
                            isTradepileFull = TradepileItems.Count >= _tradepileSize;
                        }
                        #endregion

                        #region WatchlistRoutine
                        //if (FUTSettings.UseBidSwitch && _futRequestFactory.RequestPerMinuteManager.IsNextRequestPossible())
                        //{
                        //    if (_lastWatchlistRoutine == null ||
                        //        DateTime.Now.Subtract(_lastWatchlistRoutine).TotalSeconds >= FUTSettings.WatchlistCheck)
                        //    {
                        //        await WatchlistRoutine().ConfigureAwait(false);

                        //        _lastWatchlistRoutine = DateTime.Now;
                        //    }
                        //}
                        #endregion

                        #region BuyRoutine
                        if (FUTSettings.Instance.EnableBuy && !isTradepileFull && _futRequestFactory.RequestPerMinuteManagerSearch.IsNextRequestPossible() && LogicRunning)
                        {
                            if (DateTime.Now <= _maxCardsPause)
                            {
                                continue;
                            }
                            if (FUTSettings.Instance.MaxCardsPerDay > 0)
                            {
                                var cardsBought = FUTLogsDatabase.GetFUTBuysCountLast24Hours(FUTAccount.EMail);

                                if (FUTSettings.Instance.MaxCardsPerDay <= cardsBought)
                                {
                                    UpdateStatistic("Bought more than " + FUTSettings.Instance.MaxCardsPerDay + " per day! Pausing Logic...");
                                    UpdateStatistic("Bought more than " + FUTSettings.Instance.MaxCardsPerDay + " per day! Pausing Logic...");
                                    _maxCardsPause = DateTime.Now.AddMinutes(5);
                                    continue;
                                }
                            }

                            var bidEnabled = FUTSettings.Instance.UseBidSwitch;
                            if (_pausedBidSwitchUntil != null)
                            {
                                if (_pausedBidSwitchUntil >= DateTime.Now)
                                {
                                    bidEnabled = false;
                                }
                                else
                                {
                                    UpdateStatistic("BidSwitch pausing finished! Continuing...");
                                    AddLog("BidSwitch pausing finished! Continuing...");
                                    FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "BidSwitch pausing finished! Continuing...");
                                    _pausedBidSwitchUntil = null;
                                    _biddingFailedCounter = 0;
                                    bidEnabled = true;
                                    continue;
                                }
                            }

                            if (bidEnabled)
                            {
                                if (_isBuyRoutineCounter < 3)
                                {
                                    await BidRoutine();
                                    //
                                    _isBuyRoutineCounter++;
                                }
                                else
                                {
                                    await BuyNowRoutine();
                                    //await BidRoutine();
                                    _isBuyRoutineCounter = 0;
                                }
                            }
                            else
                            {
                                await BuyNowRoutine();
                            }
                        }
                        #endregion

                        await Task.Delay(200);
                        UpdateStatistic("Idle...Waiting for next free action!");

                    }
                    catch (Exception e)
                    {
                        AddExceptionLog("Logic Exception: " + e);
                    }
                    finally
                    {
                        if (pricecheckItem != null)
                        {
                            PriceChecking = false;
                            pricecheckItem.LastPriceCheck = Helper.CreateTimestamp();
                            pricecheckItem.PriceChecking = false;
                            pricecheckItem.SaveChanges();
                        }
                    }
                }
            }
            catch (AggregateException e)
            {
            }
            catch (TaskCanceledException e)
            {
            }
            catch (Exception e)
            {
            }
            //_stopWatch.Stop();
            AddLog("Logic stopped!");
            UpdateStatistic("Logic stopped!");
            LogicRunningReal = false;
        }

        private async Task RenewCoinsAsync()
        {
            var cre = await GetCreditsAsync();
            Coins = cre;
        }

        private async Task BuyNowRoutine()
        {
            var rawItem = ItemListManager.GetNextFreeItem();
            if (rawItem == null)
            {
                await Task.Delay(1000);
                return;
            }
            if (rawItem.BuyPrice <= 0)
            {
                return;
            }

            var searchItem = rawItem.FormTransferMarketSearchObject();
            searchItem.BuyNowPrice = rawItem.BuyPrice;

            UpdateStatistic("Searching(Buy) " + rawItem.DisplayName);
            TransferMarketResponse marketResult = null;

            rawItem.TimesSearched++;
            rawItem.SaveChanges();

            var swSearch = new Stopwatch();
            swSearch.Start();
            if (searchItem.Type == FUTSearchParameterType.Player)
            {
                marketResult = await GetTransferMarketResponseAsync(searchItem);
            }
            else
            {
                if (searchItem.BuyNowPrice == 200)
                {
                    var rnd = Helper.RandomInt(0, 200);
                    if (rnd <= 100)
                    {
                        searchItem.BuyNowPrice += 50;
                    }
                    else
                    {
                        searchItem.BuyNowPrice = searchItem.BuyNowPrice;
                    }
                }
                else if (searchItem.BuyNowPrice > 200)
                {
                    var step = searchItem.BuyNowPrice.ValidatePrice(true);
                    var rnd = Helper.RandomInt(0, 300);
                    if (rnd <= 100)
                    {
                        searchItem.BuyNowPrice += step;
                    }
                    if (rnd > 100 && rnd <= 200)
                    {
                        searchItem.BuyNowPrice -= step;
                    }
                    else
                    {
                        searchItem.BuyNowPrice = searchItem.BuyNowPrice;
                    }
                }
                var random = Helper.RandomInt(1, 3);
                marketResult = await GetTransferMarketResponseAsync(searchItem, random);
            }
            //AddLog("Searching(Buy) " + rawItem.Name());
            swSearch.Stop();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            if (!marketResult.HasError && marketResult.auctionInfo != null &&
                marketResult.auctionInfo.Count > 0)
            {
                var lowestAuctions = new List<AuctionInfo>();
                if (searchItem.Type == FUTSearchParameterType.Player)
                {
                    lowestAuctions =
                        marketResult.auctionInfo.OrderByDescending(x => x.buyNowPrice).ToList();
                    lowestAuctions.Reverse();
                }
                else
                {
                    var random2 = new Random();
                    var res2 = random2.Next(0, marketResult.auctionInfo.Count - 1);
                    lowestAuctions.Add(marketResult.auctionInfo[res2]);
                }

                foreach (var result in lowestAuctions)
                {
                    var listItemDefitionID = ResourceIDManager.AssetIDToDefinitionID(
                        searchItem.AssetID, searchItem.RevisionID);
                    var resultRevisionID = ResourceIDManager.GetRevID(result.itemData.resourceId);
                    var resultAssetId = ResourceIDManager.GetAssetID(result.itemData.resourceId);
                    var resultDefinitionID = ResourceIDManager.AssetIDToDefinitionID(resultAssetId,
                        resultRevisionID);
                    if (listItemDefitionID != resultDefinitionID)
                    {
                        continue;
                    }
                    if (result.buyNowPrice > searchItem.BuyNowPrice)
                    {
                        continue;
                    }
                    if (Coins < result.buyNowPrice)
                    {
                        UpdateStatistic("Couldn't buy " + rawItem.DisplayName + " for " +
                                        result.buyNowPrice + " => no credits");
                        //AddLog("Couldn't buy " + rawItem.Name() + " for " + result.buyNowPrice +
                        //       " => no credits");
                        continue;
                    }

                    var canBuy = AuctionManager.CanBuyAuction(result.tradeId);
                    if (!canBuy)
                    {
                        UpdateStatistic("Couldn't buy " + rawItem.DisplayName + " for " +
                                        result.buyNowPrice + " => Auction already exists!");
                        //AddLog("Couldn't buy " + rawItem.Name() + " for " + result.buyNowPrice +
                        //       " => Auction already exists!");
                        continue;
                    }
                    stopWatch.Stop();
                    var elapsed = stopWatch.ElapsedMilliseconds;
                    var elapsedSearch = swSearch.ElapsedMilliseconds;

                    TimeSpan t = TimeSpan.FromSeconds(result.expires);

                    string answer = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);

                    var buyResult = await BuyTradeAsync(result.tradeId, result.buyNowPrice);
                    if (!buyResult.HasError)
                    {
                        var auction = buyResult.auctionInfo.FirstOrDefault();
                        AddLog("Buy success (id " + result.tradeId + "/" + elapsedSearch +
                               " ms" + "/" + elapsed + " ms) Expires: " + answer + " -> " +
                               rawItem.DisplayName + " " +
                               auction.buyNowPrice + " (" + rawItem?.BuyPrice + ")");
                        UpdateStatistic("Buy success (id " + result.tradeId + "/" + elapsedSearch +
                                        " ms" + "/" + elapsed + " ms) Expires: " + answer + " -> " +
                                        rawItem.DisplayName + " " +
                                        auction.buyNowPrice + " (" + rawItem?.BuyPrice +  ")");
                        var futBuy = new FUTBuy();
                        futBuy.EMail = FUTAccount.EMail;
                        futBuy.BuyNowPrice = auction.buyNowPrice;
                        futBuy.Type = FUTBuyBidType.BuyNow;
                        futBuy.AssetID = resultAssetId;
                        futBuy.RevisionID = resultRevisionID;
                        futBuy.Timestamp = Helper.CreateTimestamp();
                        futBuy.TradeID = auction.tradeId;
                        FUTLogsDatabase.InsertFUTBuy(futBuy);

                        var futItemProfit = new FUTItemProfit();
                        futItemProfit.ItemID = auction.itemData.id;
                        futItemProfit.Type = FUTBuyBidType.BuyNow;
                        futItemProfit.AssetID = resultAssetId;
                        futItemProfit.RevisionID = resultRevisionID;
                        futItemProfit.ChemistryStyle = rawItem.ChemistryStyle;
                        futItemProfit.Position = rawItem.Position;
                        futItemProfit.BuyPrice = auction.buyNowPrice;
                        futItemProfit.BuyTimestamp = Helper.CreateTimestamp();
                        futItemProfit.Account = FUTAccount.EMail;
                        FUTLogsDatabase.InsertFUTItemProfit(futItemProfit);


                        if (rawItem != null)
                        {
                            AdjustPercentages(rawItem, 1);
                        }

                        if (TradepileItems.Count < _tradepileSize)
                        {
                            UpdateStatistic("Moving bought item to tradepile");
                            var moveResult = await MoveItemToTradepileAsync(buyResult.auctionInfo[0].itemData.id);
                            if (!moveResult.HasError && moveResult.itemData != null && moveResult.itemData[0].id != 0)
                            {
                                var revID = ResourceIDManager.GetRevID(buyResult.auctionInfo[0].itemData.resourceId);
                                var assetID = ResourceIDManager.GetAssetID(buyResult.auctionInfo[0].itemData.resourceId);

                                var listItem = ItemListManager.GetMostMatchingListItem(assetID, revID,
                                    auction.itemData.playStyle, auction.itemData.preferredPosition);

                                if (listItem != null && listItem.Discard)
                                {
                                    AddLog("Discarding Item " + listItem.DisplayName);
                                    UpdateStatistic("Discarding Item " + listItem.DisplayName);
                                    await DiscardItemAsync(moveResult.itemData[0].id);
                                    continue;
                                }
                                if (listItem != null && listItem.SellPrice > 0)
                                {
                                    var listObject = new OfferItemModel(moveResult.itemData[0].id, listItem.SellPrice);
                                    var listed = await OfferItemOnTransferMarketAsync(listObject);
                                }
                                else if (listItem == null)
                                {
                                    var newObject = new FUTListItem(assetID);
                                    newObject.RevisionID = revID;
                                    newObject.BuyItem = false;
                                    newObject.Type = FUTItemManager.GetFUTSearchParamaterType(assetID, revID);
                                    newObject.LastPriceCheck = 0;
                                    ItemListManager.InsertFUTListItem(newObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        AddLog("Buy failed (id " + result.tradeId + "/" + elapsedSearch + " ms" + "/" +
                               elapsed + " ms) -> Expires: " + answer + " -> " + rawItem.DisplayName +
                               " -> " + Enum.GetName(typeof(FUTErrorCode), buyResult.Code));
                        UpdateStatistic("Buy failed (id " + result.tradeId + "/" + elapsedSearch + " ms" +
                                        "/" + elapsed + " ms) -> Expires: " + answer + " -> " +
                                        rawItem.DisplayName + " -> " +
                                        Enum.GetName(typeof(FUTErrorCode), buyResult.Code));
                    }
                    await Task.Delay(FUTSettings.Instance.WaitAfterBuy * 1000);
                    break;

                }
            }
        }

        private async Task BidRoutine()
        {
            var rawItem = ItemListManager.GetNextFreeItem();
            if (rawItem == null)
            {
                await Task.Delay(1000);
                return;
            }
            if (rawItem.BuyPrice <= 0)
            {
                return;
            }
            var searchItem = rawItem.FormTransferMarketSearchObject();
            searchItem.BidPrice = rawItem.BuyPrice;


            UpdateStatistic("Searching(Bid) " + rawItem.DisplayName);
            TransferMarketResponse marketResult = null;

            var swSearch = new Stopwatch();
            swSearch.Start();
            marketResult = await GetTransferMarketResponseAsync(searchItem);
            swSearch.Stop();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (!marketResult.HasError && marketResult.auctionInfo != null && marketResult.auctionInfo.Count > 0)
            {
                var lowestAuctions = new List<AuctionInfo>();
                lowestAuctions = marketResult.auctionInfo.Where(x => x.expires <= FUTSettings.Instance.ExpiredTimer).OrderByDescending(x => x.expires).ToList();


                foreach (var result in lowestAuctions)
                {
                    var listItemDefitionID = ResourceIDManager.AssetIDToDefinitionID(searchItem.AssetID, searchItem.RevisionID);
                    var resultRevisionID = ResourceIDManager.GetRevID(result.itemData.resourceId);
                    var resultAssetId = ResourceIDManager.GetAssetID(result.itemData.resourceId);
                    var resultDefinitionID = ResourceIDManager.AssetIDToDefinitionID(resultAssetId,
                        resultRevisionID);
                    if (listItemDefitionID != resultDefinitionID)
                    {
                        continue;
                    }
                    if (result.currentBid >= searchItem.BidPrice)
                    {
                        continue;
                    }
                    if (Coins < searchItem.BidPrice)
                    {
                        //UpdateStatistic("Couldn't bid on " + rawItem.Name() + " for " +
                        //                result.buyNowPrice + " => no credits");
                        //AddLog("Couldn't bid on " + rawItem.Name() + " for " + result.buyNowPrice +
                        //       " => no credits");
                        continue;
                    }

                    var canBuy = AuctionManager.CanBuyAuction(result.tradeId);
                    if (!canBuy)
                    {
                        UpdateStatistic("Couldn't bid on " + rawItem.DisplayName + " for " +
                                        result.buyNowPrice + " => Auction already exists!");
                        //AddLog("Couldn't bid on " + rawItem.Name() + " for " + result.buyNowPrice +
                        //       " => Auction already exists!");
                        continue;
                    }
                    stopWatch.Stop();
                    var elapsed = stopWatch.ElapsedMilliseconds;
                    var elapsedSearch = swSearch.ElapsedMilliseconds;

                    TimeSpan t = TimeSpan.FromSeconds(result.expires);

                    string answer = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);

                    var bidResult = await BuyTradeAsync(result.tradeId, searchItem.BidPrice);
                    if (!bidResult.HasError && bidResult.auctionInfo.FirstOrDefault().bidState == "highest")
                    {
                        //_currentBidCounter++;
                        _biddingFailedCounter = 0;
                        AddLog("Bid success (id " + result.tradeId + "/" + elapsedSearch + " ms" + "/" + elapsed + " ms) Expires: " + answer + " -> " + rawItem.DisplayName + " " +
                               bidResult.auctionInfo.FirstOrDefault().currentBid);

                        UpdateStatistic("Bid success (id " + result.tradeId + "/" + elapsedSearch +
                                        " ms" + "/" + elapsed + " ms) Expires: " + answer + " -> " +
                                        rawItem.DisplayName + " " +
                                        bidResult.auctionInfo.FirstOrDefault().currentBid);

                    }
                    else
                    {
                        _biddingFailedCounter++;

                        AddLog("Bid failed (id " + result.tradeId + "/" + elapsedSearch + " ms" + "/" +
                               elapsed + " ms) -> Expires: " + answer + " -> " + rawItem.DisplayName +
                               " -> " + Enum.GetName(typeof(FUTErrorCode), bidResult.Code));
                        UpdateStatistic("Bid failed (id " + result.tradeId + "/" + elapsedSearch + " ms" +
                                        "/" + elapsed + " ms) -> Expires: " + answer + " -> " +
                                        rawItem.DisplayName + " -> " +
                                        Enum.GetName(typeof(FUTErrorCode), bidResult.Code));
                        if (_biddingFailedCounter >= 30)
                        {
                            _pausedBidSwitchUntil = DateTime.Now.AddHours(2);
                            AddLog("Bidding failed 30 times, stopping bidding for 2 hours!");
                            UpdateStatistic("Bidding failed 30 times, stopping bidding for 2 hours!");
                            FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Bidding failed 30 times, stopping bidding for 2 hours!");
                        }
                    }
                    await Task.Delay(FUTSettings.Instance.WaitAfterBuy * 1000);
                    break;
                }
            }
        }

        private DateTime _lastTradepileRoutine = DateTime.Now.Subtract(new TimeSpan(24,0,0));
        private object _tradepileItemsLock = new object();
        private List<AuctionInfo> _tradepileItems;
        public List<AuctionInfo> TradepileItems
        {
            get
            {
                lock(_tradepileItemsLock)
                {
                    return _tradepileItems;
                }
            }
            set
            {
                lock (_tradepileItemsLock)
                {
                    _tradepileItems = value;
                }
            }
        }
        public int TradepileCoins => TradepileItems.Sum(x => x.buyNowPrice);

        private int _biddingFailedCounter = 0;
        private DateTime? _pausedBidSwitchUntil = null;

        private int _tradepileSize = -1;

        private List<double> _fatalBuggedCards = new List<double>();
        private bool _relistSingleTime = false;
        public bool TradepileRoutineRunning { get; set; }
        private async Task TradepileRoutine()
        {
            TradepileRoutineRunning = true;
            UpdateStatistic("Starting Tradepile routine");
            if (_tradepileSize == -1)
            {
                _tradepileSize = await GetTradepileSizeAsync();
            }


            var relistAll = false;

            var calculatedTP = -1;
            var itemsToMove = 0;

            var tradepile = await GetTradepileAsync();
            if (tradepile.auctionInfo == null)
            {
                tradepile.auctionInfo = new List<AuctionInfo>();
            }
            if (!tradepile.HasError && tradepile.auctionInfo != null)
            {
                var allItemIDs = tradepile.auctionInfo.Select(x => x.itemData.id).ToList();
                var duplicateItems = allItemIDs.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

                FUTLogsDatabase.UpdateCoinsByFUTAccount(FUTAccount, tradepile.credits);
                Coins = tradepile.credits;

                calculatedTP = tradepile.auctionInfo.Count;
                foreach (var item in tradepile.auctionInfo)
                {
                    if (item.itemData.untradeable && item.itemData.discardValue == 0)
                    {
                        continue;
                    }
                    if (item.tradeState == "closed")
                    {
                        await RemoveItemFromTradepileAsync(item.tradeId);
                        var assetID = ResourceIDManager.GetAssetID(item.itemData.resourceId);
                        var revID = ResourceIDManager.GetRevID(item.itemData.resourceId);
                        var webItem = FUTItemManager.GetItemByAssetRevisionID(assetID, revID);



                        FUTLogsDatabase.UpdateFUTItemProfitByItemID(item.itemData.id, item.buyNowPrice);

                        var itemProfit = FUTLogsDatabase.GetFUTItemProfitByItemID(item.itemData.id);

                        var sellInfos = "";
                        if (itemProfit != null)
                        {
                            var dtBuy = Helper.TimestampToDateTime(itemProfit.BuyTimestamp);
                            var dtSell = Helper.TimestampToDateTime(itemProfit.SellTimestamp);
                            var difference = dtSell.Subtract(dtBuy);
                            sellInfos = Environment.NewLine + "Bought for " + itemProfit.BuyPrice + Environment.NewLine + "Profit " + itemProfit.Profit + Environment.NewLine + "Time on TP " + difference.ToReadableString();
                        }

                        var futSell = new FUTSell();
                        futSell.EMail = FUTAccount.EMail;
                        if (itemProfit != null)
                        {
                            futSell.Type = itemProfit.Type;
                        }
                        else
                        {
                            futSell.Type = FUTBuyBidType.BuyNow;
                        }
                        futSell.SellPrice = item.buyNowPrice;
                        futSell.AssetID = assetID;
                        futSell.RevisionID = revID;
                        futSell.Timestamp = Helper.CreateTimestamp();
                        futSell.TradeID = item.tradeId;
                        FUTLogsDatabase.InsertFUTSell(futSell);

                        if (webItem != null)
                        {
                            AddLog("Item sold -> " + $"{webItem.GetName()} ({item.itemData.rating} / {revID} / {item.itemData.preferredPosition} / {(ChemistryStyle)item.itemData.playStyle})" + " for " + item.buyNowPrice + sellInfos);
                            UpdateStatistic("Item sold -> " + $"{webItem.GetName()} ({item.itemData.rating} / {revID} / {item.itemData.preferredPosition} / {(ChemistryStyle)item.itemData.playStyle})" + " " + item.buyNowPrice + sellInfos);
                        }
                        else
                        {
                            AddLog("Item sold -> " + $"{item.tradeId} ({item.itemData.rating} / {revID} / {item.itemData.preferredPosition} / {(ChemistryStyle)item.itemData.playStyle})" + "(t) " + item.buyNowPrice + sellInfos);
                            UpdateStatistic("Item sold -> " + $"{item.tradeId} ({item.itemData.rating} / {revID} / {item.itemData.preferredPosition} / {(ChemistryStyle)item.itemData.playStyle})" + item.buyNowPrice + sellInfos);
                        }


                        var listItem = ItemListManager.GetMostMatchingListItem(assetID, revID,
                            item.itemData.playStyle, item.itemData.preferredPosition);

                        if (listItem != null)
                        {
                            AdjustPercentages(listItem, -1);
                        }

                        calculatedTP--;

                    }
                    if (item.tradeState == "expired")
                    {
                        if (duplicateItems.Contains(item.itemData.id))
                        {
                            continue;
                        }
                        var revID = ResourceIDManager.GetRevID(item.itemData.resourceId);
                        var assetID = ResourceIDManager.GetAssetID(item.itemData.resourceId);

                        var listItem = ItemListManager.GetMostMatchingListItem(assetID, revID,
                            item.itemData.playStyle, item.itemData.preferredPosition);

                        if (listItem != null)
                        {
                            //AddLog("Item expired -> " + listItem.Name() + " " + item.buyNowPrice);
                            UpdateStatistic("Item expired -> " + listItem.DisplayName + " " + item.buyNowPrice);
                            if (listItem.Discard)
                            {
                                AddLog("Discarding Item " + listItem.DisplayName);
                                UpdateStatistic("Discarding Item " + listItem.DisplayName);
                                await DiscardItemAsync(item.itemData.id);
                                continue;
                            }
                        }
                        else
                        {
                            var webItem = FUTItemManager.GetItemByAssetRevisionID(assetID, revID);
                            if (webItem == null)
                            {
                                //AddLog("Item expired -> " + item.tradeId + "(t) " + item.buyNowPrice);
                                UpdateStatistic("Item expired -> " + item.tradeId + "(t) " + item.buyNowPrice);
                            }
                            else
                            {
                                //AddLog("Item expired -> " + webItem.GetName() + "(not in list) " + item.buyNowPrice);
                                UpdateStatistic("Item expired -> " + webItem.GetName() + "(not in list) " + item.buyNowPrice);
                            }
                        }
                        if (listItem != null && listItem.SellPrice > 0)
                        {
                            if (_relistSingleTime)
                            {
                                var listObject = new OfferItemModel(item.itemData.id, listItem.SellPrice);
                                var listed = await OfferItemOnTransferMarketAsync(listObject);
                            }
                            else
                            {
                                if (listItem.SellPrice != item.buyNowPrice)
                                {
                                    var listObject = new OfferItemModel(item.itemData.id, listItem.SellPrice);
                                    var listed = await OfferItemOnTransferMarketAsync(listObject);
                                }
                                else
                                {
                                    relistAll = true;
                                }
                            }
                        }
                        else if (listItem == null)
                        {
                            var newObject = new FUTListItem(assetID);
                            newObject.RevisionID = revID;
                            newObject.BuyItem = false;
                            newObject.Type = FUTItemManager.GetFUTSearchParamaterType(assetID, revID);
                            newObject.LastPriceCheck = 0;
                            ItemListManager.InsertFUTListItem(newObject);
                        }
                    }
                    if (item.tradeState == null)
                    {
                        if (duplicateItems.Contains(item.itemData.id))
                        {
                            continue;
                        }

                        var revID = ResourceIDManager.GetRevID(item.itemData.resourceId);
                        var assetID = ResourceIDManager.GetAssetID(item.itemData.resourceId);

                        var listItem = ItemListManager.GetMostMatchingListItem(assetID, revID,
                            item.itemData.playStyle, item.itemData.preferredPosition);

                        if (listItem != null && listItem.Discard)
                        {
                            AddLog("Discarding Item " + listItem.DisplayName);
                            UpdateStatistic("Discarding Item " + listItem.DisplayName);
                            await DiscardItemAsync(item.itemData.id);
                            continue;
                        }
                        if (listItem != null && listItem.SellPrice > 0)
                        {
                            var listObject = new OfferItemModel(item.itemData.id, listItem.SellPrice);
                            var listed = await OfferItemOnTransferMarketAsync(listObject);
                        }
                        else if (listItem == null)
                        {
                            var newObject = new FUTListItem(assetID);
                            newObject.RevisionID = revID;
                            newObject.BuyItem = false;
                            newObject.Type = FUTItemManager.GetFUTSearchParamaterType(assetID, revID);
                            newObject.LastPriceCheck = 0;
                            ItemListManager.InsertFUTListItem(newObject);
                        }

                    }
                    foreach (var itemID in duplicateItems)
                    {
                        if (_fatalBuggedCards.Contains(itemID))
                        {
                            continue;
                        }
                        var items = tradepile.auctionInfo.Where(x => x.itemData.id == itemID).ToList();
                        var expiredItems = items.Where(x => x.tradeState == "expired" || x.tradeState == null).ToList();
                        if (expiredItems.Count == items.Count)
                        {
                            AddLog("Discarding doubled id " + itemID);
                            FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Discarding doubled id " + itemID);
                            var result = await DiscardItemAsync(itemID);
                            if (result == false)
                            {
                                _fatalBuggedCards.Add(itemID);
                            }
                        }
                    }
                }


                FUTAccountStatistic.TradepileCount = calculatedTP;
                TradepileItems = tradepile.auctionInfo;
                itemsToMove = _tradepileSize - calculatedTP;
            }

            if (_relistSingleTime)
            {
                _relistSingleTime = false;
            }

            if (relistAll)
            {
                if (!await RelistAllAsync())
                {
                    _relistSingleTime = true;
                }
            }


            if (itemsToMove > 0)
            {
                var notAssigned = await GetNotAssignedAsync();
                if (!notAssigned.HasError && notAssigned.itemData != null)
                {
                    foreach (var item in notAssigned.itemData)
                    {
                        if (itemsToMove <= 0)
                        {
                            break;
                        }
                        if (!item.untradeable && item.discardValue != 0)
                        {
                            UpdateStatistic("Moving card " + item.id + " to tradepile!");
                            var moveResult = await MoveItemToTradepileAsync(item.id);
                            itemsToMove--;
                        }
                        if (item.discardValue <= 0)
                        {
                            await DiscardItemAsync(item.id);
                        }

                    }
                }
            }


            if (FUTSettings.Instance.UseBidSwitch)
            {
                var watchlist = await GetWatchlistAsync();
                if (!watchlist.HasError && watchlist.auctionInfo != null)
                {
                    if (watchlist.total > 0)
                    {
                        var lastCount = watchlist.total;
                        var removeableCards = watchlist.auctionInfo.Where(x => (x.tradeState == "closed" || x.tradeState == "expired" || x.expires <= 0) && x.bidState != "highest" && x.bidState != "buyNow").Select(x => x.tradeId.ToString()).Take(60).ToList();
                        while (removeableCards.Count > 0)
                        {
                            var ids = String.Join("%2C", removeableCards);

                            var removedAll = await RemoveItemsFromWatchlistAsync(removeableCards);

                            watchlist = await GetWatchlistAsync();
                            if (watchlist.total == lastCount)
                            {
                                break;
                            }
                            lastCount = watchlist.total;
                            removeableCards = watchlist.auctionInfo.Where(x => (x.tradeState == "closed" || x.tradeState == "expired" || x.expires <= 0) && x.bidState != "highest" && x.bidState != "buyNow").Select(x => x.tradeId.ToString()).Take(60).ToList();
                        }
                        //_currentBidCounter = watchlist.auctionInfo.Count;
                        foreach (var auction in watchlist.auctionInfo)
                        {
                            if (_fatalBuggedCards.Contains(auction.itemData.id))
                            {
                                continue;
                            }
                            if (auction.itemData.untradeable)
                            {
                                if (!_fatalBuggedCards.Contains(auction.itemData.id))
                                {
                                    _fatalBuggedCards.Add(auction.itemData.id);
                                    //_currentBidCounter--;
                                }
                                continue;
                            }
                            if (auction.tradeState == "closed" || auction.tradeState == "expired" || auction.expires <= 0)
                            {
                                if (auction.bidState == "highest" || auction.bidState == "buyNow")
                                {
                                    var resultAssetId = ResourceIDManager.GetAssetID(auction.itemData.resourceId);
                                    var resultRevisionID = ResourceIDManager.GetRevID(auction.itemData.resourceId);
                                    var item =
                                        ItemListManager
                                            .GetFUTListItems()
                                            .FirstOrDefault(x => x.AssetID == resultAssetId && x.RevisionID == resultRevisionID);
                                    if (item != null)
                                    {
                                        AddLog("Bidauction won (id " + auction.tradeId + " -> " + item.DisplayName + " " + auction.currentBid);
                                        UpdateStatistic("Bidauction won (id " + auction.tradeId + " -> " + item.DisplayName + " " + auction.currentBid);
                                    }
                                    else
                                    {
                                        AddLog("Bidauction won (id " + auction.tradeId + " -> " + resultAssetId + "/" + resultRevisionID + " " + auction.currentBid);
                                        UpdateStatistic("Bidauction won (id " + auction.tradeId + " -> " + resultAssetId + "/" + resultRevisionID + " " + auction.currentBid);
                                    }

                                    var futBuy = new FUTBuy();
                                    futBuy.EMail = FUTAccount.EMail;
                                    futBuy.Type = FUTBuyBidType.Bid;
                                    futBuy.BuyNowPrice = auction.currentBid;
                                    futBuy.AssetID = resultAssetId;
                                    futBuy.RevisionID = resultRevisionID;
                                    futBuy.Timestamp = Helper.CreateTimestamp();
                                    futBuy.TradeID = auction.tradeId;
                                    FUTLogsDatabase.InsertFUTBuy(futBuy);

                                    var futItemProfit = new FUTItemProfit();
                                    futItemProfit.Account = FUTAccount.EMail;
                                    futItemProfit.ItemID = auction.itemData.id;
                                    futItemProfit.Type = FUTBuyBidType.Bid;
                                    futItemProfit.AssetID = resultAssetId;
                                    futItemProfit.RevisionID = resultRevisionID;
                                    futItemProfit.ChemistryStyle = item.ChemistryStyle;
                                    futItemProfit.Position = item.Position;
                                    futItemProfit.BuyPrice = auction.currentBid;
                                    futItemProfit.BuyTimestamp = Helper.CreateTimestamp();
                                    FUTLogsDatabase.InsertFUTItemProfit(futItemProfit);


                                    if (item != null)
                                    {
                                        AdjustPercentages(item, 1);
                                    }
                                    var moveResult = await MoveItemToTradepileAsync(auction.itemData.id);
                                    //_currentBidCounter--;
                                }
                            }
                            if (auction.tradeState == "active")
                            {
                                if (auction.bidState != "highest" && auction.bidState != "buyNow")
                                {
                                    await RemoveItemsFromWatchlistAsync(new List<string>() { auction.tradeId.ToString() });
                                    //_currentBidCounter--;
                                }
                            }
                        }
                    }
                }
            }
            TradepileRoutineRunning = false;
        }

        private void AdjustPercentages(FUTListItem item, int additional = 0)
        {
            var itemsOnTP = BotManager.GetTradepileItems().Where(x => ResourceIDManager.GetAssetID(x.itemData.resourceId) == item.AssetID && ResourceIDManager.GetRevID(x.itemData.resourceId) == item.RevisionID).ToList();
            if(item.IgnorePriceCheck)
            {
                if(item.Counter > 0)
                {
                    if ((itemsOnTP.Count + additional) >= item.Counter)
                    {
                        if (item.BuyItem)
                        {
                            item.BuyItem = false;
                            item.SaveChanges();
                            AddLog("Deactivated buy on " + item.DisplayName);

                        }
                    }
                    else
                    {
                        if (!item.BuyItem)
                        {
                            item.BuyItem = true;
                            item.SaveChanges();
                            AddLog("Activating buy on " + item.DisplayName);
                        }
                    }
                }

            }
            else
            {
                if ((itemsOnTP.Count + additional) >= item.Counter)
                {
                    if (item.VariableBuyPercent == item.StaticBuyPercent && item.BuyPercentStep > 0)
                    {
                        var oldb = item.BuyPrice;
                        var p100 = item.BuyPrice / item.VariableBuyPercent;
                        item.VariableBuyPercent = item.StaticBuyPercent - item.BuyPercentStep;
                        //item.LastPriceCheck = 0;
                        item.BuyPrice = (p100 * item.VariableBuyPercent).ValidatePrice();
                        item.SaveChanges();
                        AddLog("Adjusted BuyPercent on " + item.DisplayName + " from " + item.StaticBuyPercent + " to " + item.VariableBuyPercent + " (" + oldb + "/" + item.BuyPrice + ")");
                    }
                }
                else
                {
                    if (item.VariableBuyPercent != item.StaticBuyPercent && item.BuyPercentStep > 0)
                    {
                        var oldb = item.BuyPrice;
                        var p100 = item.BuyPrice / item.VariableBuyPercent;
                        item.VariableBuyPercent = item.StaticBuyPercent;
                        //item.LastPriceCheck = 0;
                        item.BuyPrice = (p100 * item.VariableBuyPercent).ValidatePrice();
                        item.SaveChanges();
                        AddLog("Adjusted BuyPercent on " + item.DisplayName + " from " + (item.StaticBuyPercent - item.BuyPercentStep) + " to " + item.StaticBuyPercent + " (" + oldb + "/" + item.BuyPrice + ")");
                    }
                }
            }

        }

        private async Task DiscardEverything()
        {
            var notAssigned = await GetNotAssignedAsync();

            if (!notAssigned.HasError && notAssigned.itemData != null)
            {
                foreach (var item in notAssigned.itemData)
                {
                    AddLog("Discarding item " + item.id + " on not assigned!");
                    await DiscardItemAsync(item.id);
                }
            }

            var tradepile = await GetTradepileAsync();

            if (!tradepile.HasError && tradepile.auctionInfo != null)
            {
                foreach(var item in tradepile.auctionInfo)
                {
                    if(item.tradeState == null ||item.tradeState != "closed")
                    {
                        AddLog("Discarding item " + item.itemData.id + " on tradepile!");
                        await DiscardItemAsync(item.itemData.id);
                    }
                }
            }
        }

        private async Task FixGhostCardsAsync()
        {
            var tradepile = await GetTradepileAsync();

            if (!tradepile.HasError && tradepile.auctionInfo != null)
            {
                var allItemIDs = tradepile.auctionInfo.Select(x => x.itemData.id).ToList();
                var duplicateItems = allItemIDs.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                AddLog("Found " + duplicateItems.Count + " doubled cards!");

                foreach(var item in duplicateItems)
                {
                    AddLog("Discarding " + item);
                    FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Discarding " + item);
                    var result = await DiscardItemAsync(item);
                }
            }
        }

        public async Task ResetBuysSellsAsync()
        { 
            var notAssigned = await GetNotAssignedAsync();
            if (!notAssigned.HasError && notAssigned.itemData != null)
            {
                foreach (var item in notAssigned.itemData)
                {
                    var buy = new FUTBuy();
                    buy.BuyNowPrice = 0;
                    buy.EMail = FUTAccount.EMail;
                    buy.AssetID = ResourceIDManager.GetAssetID(item.resourceId);
                    buy.RevisionID = ResourceIDManager.GetRevID(item.resourceId);
                    buy.TradeID = 0;
                    buy.Timestamp = Helper.CreateTimestamp();
                    FUTLogsDatabase.InsertFUTBuy(buy);
                }
            }

            var tradepile = await GetTradepileAsync();

            if (!tradepile.HasError && tradepile.auctionInfo != null)
            {
                foreach (var item in tradepile.auctionInfo)
                {
                    var assetID = ResourceIDManager.GetAssetID(item.itemData.resourceId);
                    var revID = ResourceIDManager.GetRevID(item.itemData.resourceId);
                    var buy = new FUTBuy();
                    buy.BuyNowPrice = 0;
                    buy.EMail = FUTAccount.EMail;
                    buy.AssetID = assetID;
                    buy.RevisionID = revID;
                    buy.TradeID = 0;
                    buy.Timestamp = Helper.CreateTimestamp();
                    FUTLogsDatabase.InsertFUTBuy(buy);
                    if (item.tradeState == "closed")
                    {
                        var webItem = FUTItemManager.GetItemByAssetRevisionID(assetID, revID);
                        var futSell = new FUTSell();
                        futSell.EMail = FUTAccount.EMail;
                        futSell.SellPrice = item.buyNowPrice;
                        futSell.AssetID = assetID;
                        futSell.RevisionID = revID;
                        futSell.Timestamp = Helper.CreateTimestamp();
                        futSell.TradeID = item.tradeId;
                        FUTLogsDatabase.InsertFUTSell(futSell);

                        await RemoveItemFromTradepileAsync(item.tradeId);
                    }
                }
            }
        }

        public async Task DeleteAllSoldAuctions(bool log = false)
        {
            var tradepile = await GetTradepileAsync();
            if (!tradepile.HasError && tradepile.auctionInfo != null)
            {
                foreach (var item in tradepile.auctionInfo)
                {
                    if (item.tradeState == "closed")
                    {
                        if(log)
                        {
                            var assetID = ResourceIDManager.GetAssetID(item.itemData.resourceId);
                            var revID = ResourceIDManager.GetRevID(item.itemData.resourceId);
                            var webItem = FUTItemManager.GetItemByAssetRevisionID(assetID, revID);
                            if (webItem != null)
                            {
                                AddLog("Item sold -> " + webItem.GetName() + " " + item.buyNowPrice);
                                UpdateStatistic("Item sold -> " + webItem.GetName() + " " + item.buyNowPrice);
                            }
                            else
                            {
                                AddLog("Item sold -> " + item.tradeId + "(t) " + item.buyNowPrice);
                                UpdateStatistic("Item sold -> " + item.tradeId + "(t) " + item.buyNowPrice);
                            }

                            var futSell = new FUTSell();
                            futSell.EMail = FUTAccount.EMail;
                            futSell.SellPrice = item.buyNowPrice;
                            futSell.AssetID = assetID;
                            futSell.RevisionID = revID;
                            futSell.Timestamp = Helper.CreateTimestamp();
                            futSell.TradeID = item.tradeId;
                            FUTLogsDatabase.InsertFUTSell(futSell);
                        }
                        await RemoveItemFromTradepileAsync(item.tradeId);
                    }
                }
            }
        }
        
        private async Task<int> IntelligentPriceCheck(FUTListItem playerOriginal)
        {
            var player = playerOriginal.FormPriceCheckPlayer();
            var calculatedPrice = 0;
            var priceCheckList = new List<PriceCheckReturnResult>();
            var priceList = new List<int>();
            var counterChecks = 0;
            while (true)
            {
                var priceCheck = await PriceCheckAsyncDemo(player);
                priceCheckList.Add(priceCheck);
                priceList.Add(priceCheck.CalculatedPrice);
                if (counterChecks >= 5)
                {
                    break;
                }
                counterChecks++;

                if (priceCheck.Reason == PriceCheckReturnReason.Failed)
                {
                    return -1;
                }
                if (priceCheck.Reason == PriceCheckReturnReason.Normal)
                {
                    if (calculatedPrice == 0 || priceCheck.CalculatedPrice < calculatedPrice)
                    {
                        calculatedPrice = priceCheck.CalculatedPrice;
                        player.BuyPrice = calculatedPrice;
                    }

                    continue;
                }
                if (priceCheck.Reason == PriceCheckReturnReason.LessThanXXPlayers)
                {
                    if (calculatedPrice == 0 || priceCheck.CalculatedPrice < calculatedPrice)
                    {
                        calculatedPrice = priceCheck.CalculatedPrice;
                    }
                    break;
                }
            }

            return calculatedPrice;
        }
       
        private async Task<PriceCheckReturnResult> PriceCheckAsyncDemo(PriceCheckItemModel player)
        {
            var errorCounter = 0;
            var page = 1;
            var results = new List<AuctionInfo>();
            var returnReason = PriceCheckReturnReason.Normal;
            while (true)
            {
                var res = await GetTransferMarketResponseAsync(player, page);
                if (res.HasError || res.auctionInfo == null)
                {
                    if (errorCounter >= 5)
                    {
                        break;
                    }
                    if(res.Code == FUTErrorCode.ExpiredSession)
                    {

                    }
                    errorCounter++;
                    continue;
                }
                
                results.AddRange(res.auctionInfo);
                if (res.auctionInfo.Count < 16 && page == 1)
                {
                    returnReason = PriceCheckReturnReason.LessThanXXPlayers;
                    break;
                }
                if (res.auctionInfo.Count < 16 && page > 1)
                {
                    returnReason = PriceCheckReturnReason.LessThanXXPlayers;
                    break;
                }
                if(page > 1)
                {
                    returnReason = PriceCheckReturnReason.Normal;
                    break;
                }
                page++;
                //else
                //{
                //    returnReason = PriceCheckReturnReason.Normal;
                //    break;
                //}
            }

            if (errorCounter >= 5)
            {
                return new PriceCheckReturnResult(-1, PriceCheckReturnReason.Failed);
            }

            var cheapestCards = new List<int>();
            foreach (var auction in results)
            {
                if (auction.tradeState == "active")
                {
                    cheapestCards.Add(auction.buyNowPrice);
                }

            }
            cheapestCards.Sort();

            //var totalCoins = 0;
            //var counter = player.Counter;
            //if (player.Counter > cheapestCards.Count)
            //{
            //    counter = cheapestCards.Count;
            //}
            //for (var i = 0; i <= counter - 1; i++)
            //{
            //    totalCoins += cheapestCards[i];
            //}

            //var avgPrice = -1;
            //if (counter != 0)
            //{
            //    avgPrice = ((int)Math.Round(totalCoins / (double)counter, 0)).ValidatePrice();
            //}
            return new PriceCheckReturnResult(cheapestCards.FirstOrDefault(), returnReason);
        }

        public async Task<int> PriceCheckAsyncAverage(FUTListItem playerOriginal)
        {
            var player = playerOriginal.FormPriceCheckPlayer();
            player.MinBuyNow = 200;

            var errorCounter = 0;
            var page = 1;
            var results = new List<AuctionInfo>();

            while (true)
            {
                var res = await GetTransferMarketResponseAsync(player, page);
                if (res.HasError || res.auctionInfo == null)
                {
                    if (errorCounter >= 5)
                    {
                        break;
                    }
                    errorCounter++;
                    continue;
                }

                results.AddRange(res.auctionInfo);
                page++;
                if (res.auctionInfo.Count < 16 && page == 1)
                {
                    break;
                }
                if (res.auctionInfo.Count < 16 && page > 1)
                {
                    break;
                }
            }
            
            if (errorCounter >= 5)
            {
                return -1;
            }
            AddLog(playerOriginal.DisplayName + " Existing Cards: " + results.Count);

            var cheapestCards = new List<int>();
            foreach (var auction in results)
            {
                cheapestCards.Add(auction.buyNowPrice);
            }
            cheapestCards.Sort();

            var totalCoins = 0;
            var counter = FUTSettings.Instance.Counter;
            if (FUTSettings.Instance.Counter > cheapestCards.Count)
            {
                counter = cheapestCards.Count;
            }
            for (var i = 0; i <= counter - 1; i++)
            {
                AddLog(playerOriginal.DisplayName + " Cheapest Card #" + i + " => " + cheapestCards[i]);
                totalCoins += cheapestCards[i];
            }

            var avgPrice = -1;
            if (counter != 0)
            {
                avgPrice = ((int)Math.Round(totalCoins / (double)counter, 0)).ValidatePrice();
            }
            AddLog(playerOriginal.DisplayName + " Average: " + avgPrice);
            return avgPrice;
        }

        public async Task<int> PriceCheckPlayerCalculation(FUTListItem playerOriginal)
        {
            var prevCheck = FUTLogsDatabase.GetFUTPriceChecksByAssetIDRevisionID(playerOriginal.AssetID,
                playerOriginal.RevisionID, 1).FirstOrDefault();

            var cardAmount = 0;
            var priceForRanges = 0;
            if (prevCheck == null || (DateTime.Now.Subtract(prevCheck.Timestamp.ToDateTime()).TotalMinutes > 60))
            {
                var pages = await SearchPagesAsync(playerOriginal.FormPriceCheckPlayer(), 3);
                var cheapestCard = pages.OrderBy(x => x.buyNowPrice).FirstOrDefault();
                if (cheapestCard == null)
                {
                    return -1;
                }
                priceForRanges = cheapestCard.buyNowPrice;
            }
            else
            {
                priceForRanges = prevCheck.BuyPrice;
            }
            cardAmount = CardsRequiredTable.GetRange(priceForRanges, playerOriginal.Rating);
            if (cardAmount <= 0)
            {
                //check in 12 min again
                return -1;
            }

            var maxCards = await SearchMaxCardsAsync(playerOriginal.FormPriceCheckPlayer(), cardAmount);
            maxCards = maxCards.OrderBy(x => x.buyNowPrice).ToList();
            if (maxCards.Count < cardAmount)
            {
                //check in 12 min again
                return -1;
            }

            var avg1CalcCardsRange = 5;//FirstAverageTable.GetRange(priceForRanges, playerOriginal.Rating);
            var avg1CalcCards = maxCards.Take(avg1CalcCardsRange);
            var avg1Value = ((int)Math.Round(avg1CalcCards.Sum(x => x.buyNowPrice) / (double)avg1CalcCardsRange, 0)).ValidatePrice();
            return avg1Value;
        }

        public async Task<int> PriceCheckPlayerFast(FUTListItem playerOriginal)
        {
            var player = playerOriginal.FormPriceCheckPlayer();
            player.MinBuyNow = 200;

            var errorCounter = 0;
            var cheapestAuctions = new List<AuctionInfo>();
            var foundZeroCard = false;
            var roundCounter = 0;
            while (true)
            {
                UpdateStatistic($"Pricecheck fast round {++roundCounter} {playerOriginal.DisplayName}");
                var res = await GetTransferMarketResponseAsync(player);
                if (res.HasError)
                {
                    if (errorCounter >= 5)
                    {
                        break;
                    }
                    errorCounter++;
                    continue;
                }
                if (res.auctionInfo != null && res.auctionInfo.Count >= 16)
                {
                    if (player.BuyPrice == 0)
                    {
                        var cheapestCard = res.auctionInfo.Where(x => x.buyNowPrice > 0)
                            .OrderBy(x => x.buyNowPrice).FirstOrDefault();
                        if (cheapestCard != null)
                        {
                            player.BuyPrice = cheapestCard.buyNowPrice;
                        }
                    }
                    else
                    {
                        var prices = res.auctionInfo.Select(x => x.buyNowPrice).OrderBy(x => x).ToList();
                        if (foundZeroCard)
                        {
                            cheapestAuctions.AddRange(res.auctionInfo);
                            break;
                        }
                        else
                        {
                            var couldBePrice = player.BuyPrice.DecrementPrice();
                            var cheapestFound = prices.FirstOrDefault();
                            
                            player.BuyPrice = couldBePrice < cheapestFound ? couldBePrice : cheapestFound;
                        }
                    }
                    continue;
                }
                if (res.auctionInfo == null || (res.auctionInfo.Count < 16 | res.auctionInfo.Count == 0))
                {
                    if (roundCounter == 1)
                    {
                        cheapestAuctions.AddRange(res.auctionInfo);
                        break;
                    }
                    foundZeroCard = true;
                    if (res.auctionInfo != null && res.auctionInfo.Count > 0)
                    {
                        var prices = res.auctionInfo.Select(x => x.buyNowPrice).OrderBy(x => x).ToList();
                        var couldBePrice = player.BuyPrice.IncrementPrice();
                        var cheapestFound = prices.FirstOrDefault().IncrementPrice();
                        player.BuyPrice = couldBePrice > cheapestFound ? couldBePrice : cheapestFound;
                    }
                    else if (res.auctionInfo != null && res.auctionInfo.Count >= FUTSettings.Instance.Counter)
                    {
                        cheapestAuctions.AddRange(res.auctionInfo);
                        break;
                    }
                    else
                    {
                        player.BuyPrice = player.BuyPrice.IncrementPrice();
                    }
                    continue;
                }

                if (roundCounter > 15)
                {
                    cheapestAuctions.AddRange(res.auctionInfo);
                    break;
                }
            }

            if (errorCounter >= 5)
            {
                return -1;
            }

            if (cheapestAuctions.Count <= FUTSettings.Instance.MinimumPlayersForPriceCheck)
            {
                return -1;
            }

            cheapestAuctions = cheapestAuctions.OrderBy(x => x.buyNowPrice).ToList();

            var totalCoins = 0;
            var counter = FUTSettings.Instance.Counter;
            if (counter > cheapestAuctions.Count)
            {
                counter = cheapestAuctions.Count;
            }
            for (var i = 0; i <= counter - 1; i++)
            {
                AddLog(playerOriginal.DisplayName + " Cheapest Card #" + i + " => " + cheapestAuctions[i].buyNowPrice);
                totalCoins += cheapestAuctions[i].buyNowPrice;
            }

            var avgPrice = -1;
            if (counter != 0)
            {
                avgPrice = ((int)Math.Round(totalCoins / (double)counter, 0)).ValidatePrice();
            }
            AddLog(playerOriginal.DisplayName + " Average: " + avgPrice);
            UpdateStatistic($"Pricecheck fast finished round {roundCounter} {playerOriginal.DisplayName}");
            AddLog($"Pricecheck fast finished round {roundCounter} {playerOriginal.DisplayName}");
            return avgPrice;
        }

        public void AddLog(string data)
        {
            var logData = new FUTBotLog(FUTAccount.EMail, data);
            FUTLogsDatabase.InsertFUTBotLog(logData);
        }
        public void AddExceptionLog(string data)
        {
            try
            {
                var logData = new FUTExceptionLog(FUTAccount.EMail, data);
                FUTLogsDatabase.InsertFUTExceptionLog(logData);
            }
            catch { }
        }
        public void UpdateStatistic(string data)
        {
            FUTAccountStatistic.EMail = FUTAccount.EMail;
            FUTAccountStatistic.LastActionData = data;
            FUTAccountStatistic.LastActionTimestamp = Helper.CreateTimestamp();
            FUTAccountStatistic.Update();
        }

        #region Login
        private int _loginCounterFailed = 0;
        private static object _loginSemaphoreLockObject = new object();
        public async Task<Tuple<bool, LoginResponse>> LoginAsync()
        {
            ResetRequestFactory();
            //FUTAccountsDatabase.RemoveFUTCookiesByEMail(FUTAccount.EMail);
            AsyncSemaphore semaphore = null;
            IFUTRequest<LoginResponse> loginRequest = null;
            var loginString = "";
            if (LoginMethod == LoginMethod.Web)
            {
                loginRequest = _futRequestFactory.LoginWebRequestFactory();
                loginString = "WebLogin";
            }
            else if (LoginMethod == LoginMethod.IOS)
            {
                loginRequest = _futRequestFactory.LoginIOSRequestFactory();
                loginString = "IOSLogin";
            }
            else
            {
                loginRequest = _futRequestFactory.LoginAndroidRequestFactory();
                loginString = "ANDLogin";
            }
           
            try
            {
                UpdateStatistic($"Starting {loginString}...");
                AddLog($"Starting {loginString}...");

                //FUTAccountsDatabase.RemoveFUTCookiesByEMail(FUTAccount.EMail);

                UpdateStatistic("Getting LoginSemaphore...");
                lock (_loginSemaphoreLockObject)
                {
                    semaphore = FUTSettings.Instance.OneParallelLogin ? LoginManager.GetSemaphoreForExe() : LoginManager.GetSemaphoreForProxy(FUTProxy);
                }
                UpdateStatistic("Waiting for next LoginSemaphore...");
                await semaphore.WaitAsync();
                UpdateStatistic("Waiting PauseBetweenLogins...");
                await Task.Delay(FUTSettings.Instance.PauseBetweenRelogs * 1000).ConfigureAwait(false);
                UpdateStatistic($"Performing {loginString}...");
                var loginResponse = await loginRequest.PerformRequestAsync().ConfigureAwait(false);
                FUTAccountStatistic.TotalLogins++;
                if (loginResponse.HasError)
                {
                    _loginCounterFailed++;
                    LoggedIn = false;
                    if (loginResponse.Code == FUTErrorCode.WrongEMailPassword)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "EMail Username and/or Password wrong!");
                    }
                    if (loginResponse.Code == FUTErrorCode.AccountBanned)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Account is probably banned. Please check manually!");
                    }
                    if (loginResponse.Code == FUTErrorCode.LoginFailureUrlError)
                    {
                        CookieManager.DeleteCookieContainer(FUTAccount);
                        CookieManager.DeleteCookieContainer(FUTAccount, false);
                        await Task.Delay(10000);
                    }
                    if (loginResponse.Code == FUTErrorCode.WrongLoginData)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Origin EMail and/or password wrong!");
                    }
                    if (loginResponse.Code == FUTErrorCode.NoEaswID)
                    {
                        CookieManager.DeleteCookieContainer(FUTAccount);
                        CookieManager.DeleteCookieContainer(FUTAccount, false);
                        await Task.Delay(10000);
                    }
                    if (loginResponse.Code == FUTErrorCode.UnknownEMailProvider)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Unknown EMail Provider!");
                    }
                    if (loginResponse.Code == FUTErrorCode.GMXBlocked)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "GMX is blocked!");
                    }
                    if (loginResponse.Code == FUTErrorCode.WrongSecurityAnswer)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Wrong SecurityAnswer!");
                    }
                    if (loginResponse.Code == FUTErrorCode.ServerMaintenance)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "ServerMaintenance! Stopping Logic");
                    }
                    if (loginResponse.Code == FUTErrorCode.TwoFactorFailed)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "TwoFactorCode failed! Please check for spam mails!");
                    }
                    if (loginResponse.Code == FUTErrorCode.NoUserAccounts)
                    {
                        StopLogic();
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "No Useraccounts! Bot stopped!");
                    }
                    AddLog($"{loginString} failed -> (" + Enum.GetName(typeof(FUTErrorCode), loginResponse.Code) + ") " + loginResponse.Message);
                    UpdateStatistic($"{loginString} failed -> " + loginResponse.Message);
                    if (_loginErrorCollection.IncrementAndCheck())
                    {
                        AddLog("Login Error occurred 5 times in less than 3 minutes!!! Stopping Logic!");
                        UpdateStatistic("Login Error occurred 5 times in less than 3 minutes!!! Stopping Logic!");
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Login Error occurred 5 times in less than 3 minutes, Logic stopped!");
                        StopLogic();
                    }
                    if (_loginCounterFailed >= 3)
                    {
                        AddLog("Login Error occurred 3 times!!! Stopping Logic!");
                        UpdateStatistic("Login Error occurred 3 times!!! Stopping Logic!");
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Login Error occurred 3 times!!! Stopping Logic!");
                        StopLogic();
                    }
                    return new Tuple<bool, LoginResponse>(false, loginResponse);
                }
                if (string.IsNullOrEmpty(loginResponse.SessionID))
                {
                    LoggedIn = false;
                    return new Tuple<bool, LoginResponse>(false, loginResponse);
                }
                AddLog($"{loginString} success!");
                _loginCounterFailed = 0;
                _loginErrorCollection.Reset();
                LoggedIn = true;
                UpdateStatistic($"{loginString} success!");

                FUTAccount.SaveChanges();

                if (!await GetTradingStatusEnabledAsync())
                {
                    AddLog("Trading is not enabled! Stopping Logic!");
                    UpdateStatistic("Trading is not enabled! Stopping Logic!");
                    FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Trading is not enabled! Stopping Logic!");
                    StopLogic();
                }

                return new Tuple<bool, LoginResponse>(true, loginResponse);
            }
#pragma warning disable CS0168
            catch (Exception e)
            {
                LoggedIn = false;
                return new Tuple<bool, LoginResponse>(false, new LoginResponse() { Code = FUTErrorCode.ServiceUnavailable });
            }
#pragma warning restore CS0168
            finally
            {
                if (semaphore != null)
                {
                    semaphore.Release();
                }
            }
        }
        #endregion

        #region Requests
        public async Task<bool> GetTradingStatusEnabledAsync()
        {
            var result = true;
            var user = await GetUserAsync();
            if (user.feature != null)
            {
                Coins = user.credits;
                if (user.feature.trade == 0)
                {
                    result = false;
                }
            }
            return result;
        }
        public async Task<int> GetTradepileSizeAsync()
        {
            try
            {
                var pileSizes = await GetPileSizesAsync();
                if (pileSizes.HasError)
                {
                    return 30;
                }
                if (pileSizes.entries[PileSizes.PileTrade] == null)
                {
                    return 30;
                }
                var tradePileSize = pileSizes.entries[PileSizes.PileTrade].value;
                return tradePileSize;
            }
            catch { return 30; }
        }
        public async Task<int> GetCreditsAsync()
        {
            var creditsRequest = _futRequestFactory.CreditsRequestFactory();
            var creditsResponse = await creditsRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!creditsResponse.HasError)
            {
                FUTLogsDatabase.UpdateCoinsByFUTAccount(FUTAccount, creditsResponse.credits);
                Coins = creditsResponse.credits;
                return creditsResponse.credits;
            }
            else
            {
                await HandleWebError(creditsResponse);
                return -1;
            }
           
        }
        public async Task<UserHistoricalResponse> GetUserHistoricalAsync()
        {
            var creditsRequest = _futRequestFactory.UserHistoricalRequestFactory();
            var creditsResponse = await creditsRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!creditsResponse.HasError)
            {
                return creditsResponse;
            }
            else
            {
                await HandleWebError(creditsResponse);
                return null;
            }

        }
        public async Task<UserResponse> SetNewUserAsync(UserHistoricalResponse hist)
        {
            var creditsRequest = _futRequestFactory.SetNewUserRequestFactory(hist);
            var creditsResponse = await creditsRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!creditsResponse.HasError)
            {
                return creditsResponse;
            }
            else
            {
                await HandleWebError(creditsResponse);
                return null;
            }

        }
        public async Task<TradepileResponse> GetTradepileAsync()
        {
            var tradepileRequest = _futRequestFactory.TradepileRequestFactory();
            var tradepileResponse = await tradepileRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!tradepileResponse.HasError)
            {
                FUTLogsDatabase.UpdateCoinsByFUTAccount(FUTAccount, tradepileResponse.credits);
                Coins = tradepileResponse.credits;
            }
            await HandleWebError(tradepileResponse);
            return tradepileResponse;
        }
        public async Task<bool> RelistAllAsync()
        {
            var relistRequest = _futRequestFactory.RelistAllRequestFactory();
            var relistResponse = await relistRequest.PerformRequestAsync().ConfigureAwait(false);
            return relistResponse;
        }
        public async Task<WatchlistResponse> GetWatchlistAsync()
        {
            var watchlistRequest = _futRequestFactory.WatchlistRequestFactory();
            var watchlistResponse = await watchlistRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!watchlistResponse.HasError)
            {
                FUTLogsDatabase.UpdateCoinsByFUTAccount(FUTAccount, watchlistResponse.credits);
                Coins = watchlistResponse.credits;
            }
            await HandleWebError(watchlistResponse);
            return watchlistResponse;
        }
        public async Task<NotAssignedResponse> GetNotAssignedAsync()
        {
            var notAssignedRequest = _futRequestFactory.NotAssignedRequestFactory();
            var notAssignedResponse = await notAssignedRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(notAssignedResponse);
            return notAssignedResponse;
        }
        public async Task<TransferMarketResponse> GetTransferMarketResponseAsync(FUTSearchParameter item, int page = 1, bool muling = false)
        {
            var transferMarketRequestB = _futRequestFactory.TransferMarketRequestFactory(item, muling);
            var transferMarketRequest = (TransferMarketRequest)transferMarketRequestB;
            transferMarketRequest.SetPage(page);
            var transferMarketResponse = await transferMarketRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!transferMarketResponse.HasError)
            {
                if (transferMarketResponse.credits > 0)
                {
                    FUTLogsDatabase.UpdateCoinsByFUTAccount(FUTAccount, transferMarketResponse.credits);
                    Coins = transferMarketResponse.credits;
                }

            }
            await HandleWebError(transferMarketResponse);
            return transferMarketResponse;
        }
        public async Task<TransferMarketResponse> GetTransferMarketResponseMulingAsync(FUTSearchParameter item, int page = 1)
        {
            var transferMarketRequestB = _futRequestFactory.TransferMarketRequestFactory(item, true);
            var transferMarketRequest = (TransferMarketRequest)transferMarketRequestB;
            transferMarketRequest.SetPage(page);
            var transferMarketResponse = await transferMarketRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!transferMarketResponse.HasError)
            {
                if (transferMarketResponse.credits > 0)
                {
                    FUTLogsDatabase.UpdateCoinsByFUTAccount(FUTAccount, transferMarketResponse.credits);
                    Coins = transferMarketResponse.credits;
                }

            }
            await HandleWebError(transferMarketResponse);
            return transferMarketResponse;
        }

        public async Task<List<AuctionInfo>> SearchPagesAsync(FUTSearchParameter item, int maxPage = 1)
        {
            var errorCounter = 0;
            var page = 1;
            var results = new List<AuctionInfo>();

            while (true)
            {
                var res = await GetTransferMarketResponseAsync(item, page);
                if (res.HasError || res.auctionInfo == null)
                {
                    if (errorCounter >= 5)
                    {
                        break;
                    }
                    errorCounter++;
                    continue;
                }

                results.AddRange(res.auctionInfo);
                
                if (res.auctionInfo.Count < 16 && page == 1)
                {
                    break;
                }
                if (res.auctionInfo.Count < 16 && page > 1)
                {
                    break;
                }
                if (page >= maxPage)
                {
                    break;
                }
                page++;
            }
            return results;
        }
        public async Task<List<AuctionInfo>> SearchMaxCardsAsync(FUTSearchParameter item, int maxCards = 1)
        {
            var errorCounter = 0;
            var page = 1;
            var results = new List<AuctionInfo>();

            while (true)
            {
                var res = await GetTransferMarketResponseAsync(item, page);
                if (res.HasError || res.auctionInfo == null)
                {
                    if (errorCounter >= 5)
                    {
                        break;
                    }
                    errorCounter++;
                    continue;
                }

                results.AddRange(res.auctionInfo);

                if (res.auctionInfo.Count < 16 && page == 1)
                {
                    break;
                }
                if (res.auctionInfo.Count < 16 && page > 1)
                {
                    break;
                }
                if (results.Count >= maxCards)
                {
                    break;
                }
                page++;
            }
            return results;
        }
        public async Task<List<AuctionInfo>> SearchAllItems(FUTSearchParameter item)
        {
            var errorCounter = 0;
            var page = 1;
            var results = new List<AuctionInfo>();

            while (true)
            {
                var res = await GetTransferMarketResponseAsync(item, page);
                if (res.HasError || res.auctionInfo == null)
                {
                    if (errorCounter >= 5)
                    {
                        break;
                    }
                    errorCounter++;
                    continue;
                }

                results.AddRange(res.auctionInfo);
                page++;
                if (res.auctionInfo.Count < 16 && page == 1)
                {
                    break;
                }
                if (res.auctionInfo.Count < 16 && page > 1)
                {
                    break;
                }
            }
            return results;
        }
        public async Task<AuctionInfo> SearchForItemByTradeID(FUTSearchParameter item, long tradeID)
        {
            var errorCounter = 0;
            var page = 1;
            AuctionInfo result = null;

            while (true)
            {
                var res = await GetTransferMarketResponseAsync(item, page, true);
                if (res.HasError || res.auctionInfo == null)
                {
                    if (errorCounter >= 5)
                    {
                        break;
                    }
                    errorCounter++;
                    continue;
                }

                result = res.auctionInfo.FirstOrDefault(x => x.tradeId == tradeID);
                if (result != null)
                {
                    break;
                }
                if (res.auctionInfo.Count < 16 && page == 1)
                {
                    break;
                }
                if (res.auctionInfo.Count < 16 && page > 1)
                {
                    break;
                }
                page++;
            }
            return result;
        }

        public async Task<BuyItemResponse> BuyTradeAsync(long tradeID, long price)
        {
            var buyTradeRequest = _futRequestFactory.BuyTradeRequestFactory(tradeID, price);
            var buyTradeResponse = await buyTradeRequest.PerformRequestAsync().ConfigureAwait(false);
            if (!buyTradeResponse.HasError)
            {
                FUTLogsDatabase.UpdateCoinsByFUTAccount(FUTAccount, buyTradeResponse.credits);
                Coins = buyTradeResponse.credits;
            }
            await HandleWebError(buyTradeResponse);
            return buyTradeResponse;
        }
        public async Task<MoveItemResponse> MoveItemToTradepileAsync(long itemID)
        {
            var moveRequest = _futRequestFactory.MoveItemToTradepileRequestFactory(itemID);
            var moveResponse = await moveRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(moveResponse);
            return moveResponse;
        }
        public async Task<OfferItemOnTransferMarketResponse> OfferItemOnTransferMarketAsync(OfferItemModel item)
        {
            var offerRequest = _futRequestFactory.OfferItemonTransferMarketRequestFactory(item);
            var offerResponse = await offerRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(offerResponse);
            return offerResponse;
        }
        public async Task<bool> RemoveItemFromTradepileAsync(long tradeID)
        {
            var removeRequest = _futRequestFactory.RemoveItemFromTradepileRequestFactory(tradeID);
            var removeResponse = await removeRequest.PerformRequestAsync().ConfigureAwait(false);
            return removeResponse;
        }
        public async Task<bool> RemoveItemsFromWatchlistAsync(List<string> tradeIDs)
        {
            var removeRequest = _futRequestFactory.RemoveItemsFromWatchlistRequestFactory(tradeIDs);
            var removeResponse = await removeRequest.PerformRequestAsync().ConfigureAwait(false);
            return removeResponse;
        }
        public async Task<PriceLimitsResponse> GetPriceLimitsForItemAsync(long assetID)
        {
            var priceLimitsRequest = _futRequestFactory.PriceLimitsRequestFactory(assetID);
            var priceLimitsResponse = await priceLimitsRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(priceLimitsResponse);
            return priceLimitsResponse;
        }
        public async Task<bool> DiscardItemAsync(long id, bool muling = false)
        {
            var discardItemRequest = _futRequestFactory.DiscardItemRequestFactory(id, muling);
            var discardItemResponse = await discardItemRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(discardItemResponse);
            return !discardItemResponse.HasError;
        }
        public async Task<SettingsResponse> GetSettingsAsync()
        {
            var settingsRequest = _futRequestFactory.SettingsRequestFactory();
            var settingsResponse = await settingsRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(settingsResponse);
            return settingsResponse;
        }
        public async Task<UserResponse> GetUserAsync()
        {
            var userRequest = _futRequestFactory.UserRequestFactory();
            var userResponse = await userRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(userResponse);
            return userResponse;
        }
        public async Task<PileSizeResponse> GetPileSizesAsync()
        {
            var pileRequest = _futRequestFactory.PileSizeRequestFactory();
            var pileResponse = await pileRequest.PerformRequestAsync().ConfigureAwait(false);
            await HandleWebError(pileResponse);
            return pileResponse;
        }
        #endregion

        #region ErrorHandling
        private ErrorCollection _loginErrorCollection = new ErrorCollection(5, 3);
        private ErrorCollection _proxyErrorCollection = new ErrorCollection(5, 3);
        private ErrorCollection _jsonErrorCollection = new ErrorCollection(10, 3);
        private ErrorCollection _internalServerErrorErrorCollection = new ErrorCollection(5, 3);
        private ErrorCollection _badRequestErrorCollection = new ErrorCollection(5, 3);
        private ErrorCollection _invalidCookieErrorCollection = new ErrorCollection(2, 3);
        private ErrorCollection _accountKickedErrorCollection = new ErrorCollection(3, 5);
        private ErrorCollection _captchaFailedErrorCollection = new ErrorCollection(2, 5);
        #endregion

        private FUTError _lastRequest;
        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        private async Task<bool> HandleWebError<T>(T futResponse) where T : FUTError
        {
            _lastRequest = futResponse;
            if (futResponse.HasError)
            {
                if (futResponse.Code == FUTErrorCode.ExpiredSession)
                {
                    ResetRequestFactory();
                    LoggedIn = false;
                    AddLog("Account kicked out! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                    if (_accountKickedErrorCollection.IncrementAndCheck())
                    {
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Account kicked 3 times in 5 minutes! Logic stopped!");
                        AddLog("Account kicked 3 times in 5 minutes! Logic stopped!");
                        StopLogic();
                    }
                    UpdateStatistic("Account kicked out!");
                }
                else if (futResponse.Code == FUTErrorCode.CaptchaTriggered || futResponse.Code == FUTErrorCode.CaptchaTriggered2 || futResponse.Code == FUTErrorCode.CaptchaException)
                {
                    AddLog("Captcha Triggered! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                    UpdateStatistic("Captcha Triggered! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                    FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Captcha Triggered!");
                    var solver = new CaptchaSolver(FUTProxy);
                    var result = await solver.DoAntiCaptcha();
                    if (result.errorId == 0)
                    {
                        var solveRequest = _futRequestFactory.SolveCaptchaRequestFactory(result.solution.token);
                        var solveResponse = await solveRequest.PerformRequestAsync().ConfigureAwait(false);
                        AddLog("Captcha solved successfully");
                        UpdateStatistic("Captcha solved successfully");
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Captcha solved successfully!");
                    }
                    else
                    {
                        AddLog("Solving Captcha failed! " + result.errorDescription);
                        UpdateStatistic("Solving Captcha failed! " + result.errorDescription);
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Solving Captcha failed! " + result.errorDescription);
                        if (_captchaFailedErrorCollection.IncrementAndCheck())
                        {
                            AddLog("Solving Captcha failed 2 times in 5 minutes! Pausing Logic for 1 hour!");
                            UpdateStatistic("Solving Captcha failed 2 times in 5 minutes! Pausing Logic for 1 hour!");
                            FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Solving Captcha failed 2 times in 5 minutes! Pausing Logic for 1 hour!");
                            PauseLogic(1);
                        }
                    }
                }
                #region Proxy
                else if (futResponse.Code == FUTErrorCode.ProxyException)
                {
                    AddExceptionLog("PROXYEXCEPTION:\r\n\r\n" + futResponse.Message);
                    if (_proxyErrorCollection.IncrementAndCheck())
                    {
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "5x Proxy is probably down error in 3 minutes!  Pausing Logic for 1 hour!\r\nProxyData: " + FUTProxy?.ToString());
                        AddLog("5x Proxy is probably down error in 3 minutes! Pausing account...");
                        PauseLogic(1);
                    }

                }
                else if(futResponse.Code == FUTErrorCode.HttpRequestException)
                {
                    AddExceptionLog("HTTPREQUESTEXCEPTION:\r\n\r\n" + futResponse.Message);
                    if (_proxyErrorCollection.IncrementAndCheck())
                    {
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "5x Proxy is probably down error in 3 minutes!  Pausing Logic for 1 hour!\r\nProxyData: " + FUTProxy?.ToString());
                        AddLog("5x Proxy is probably down error in 3 minutes! Pausing account...");
                        PauseLogic(1);
                    }
                }
                else if(futResponse.Code == FUTErrorCode.RequestException)
                {
                    AddExceptionLog("REQUESTEXCEPTION:\r\n\r\n" + futResponse.Message);
                    if (_proxyErrorCollection.IncrementAndCheck())
                    {
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "5x Proxy is probably down error in 3 minutes!  Pausing Logic for 1 hour!\r\nProxyData: " + FUTProxy?.ToString());
                        AddLog("5x Proxy is probably down error in 3 minutes! Pausing account...");
                        PauseLogic(1);
                    }
                }
                #endregion
                else if(futResponse.Code == FUTErrorCode.Conflict)
                {
                    AddLog("Conflict Error!!! Pausing Logic for 1 hour! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                    UpdateStatistic("Conflict Error!!! Pausing Logic for 1 hour! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                    FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Conflict Error!!! Pausing Logic for 1 hour!");
                    PauseLogic(1);
                }
                else if(futResponse.Code == FUTErrorCode.JsonSerializationException)
                {
                    AddExceptionLog(Enum.GetName(typeof(FUTErrorCode), futResponse.Code) + " -> " + futResponse.Message);
                    if (_jsonErrorCollection.IncrementAndCheck())
                    {
                        AddLog("Json Error occured 10 times in 3 minutes! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                        UpdateStatistic("Json Error occured 10 times in 3 minutes! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Json Error occured 10 times in 3 minutes!!! Pausing Logic for 1 hour!");
                        PauseLogic(1);
                    }
                }
                else if(futResponse.Code == FUTErrorCode.InternalServerError)
                {
                    if (_internalServerErrorErrorCollection.IncrementAndCheck())
                    {
                        AddLog("500 Error occured 5 times in 3 minutes!!! Pausing Logic for 1 hour!");
                        UpdateStatistic("500 Error occured 5 times in 3 minutes!!! Pausing Logic for 1 hour!");
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "500 Error occured 5 times in 3 minutes!!! Pausing Logic for 1 hour!");
                        PauseLogic(1);
                    }
                }
                else if(futResponse.Code == FUTErrorCode.BadRequest)
                {
                    AddExceptionLog("BADREQUEST:\r\n\r\n" + futResponse.Message);
                    if (_badRequestErrorCollection.IncrementAndCheck())
                    {
                        AddLog("BadRequest Error occured 5 times in 3 minutes!!! Pausing Logic for 1 hour!");
                        UpdateStatistic("BadRequest Error occured 5 times in 3 minutes!!! Pausing Logic for 1 hour!");
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "BadRequest Error occured 5 times in 3 minutes!!! Pausing Logic for 1 hour!");
                        PauseLogic(1);
                    }
                }
                else if (futResponse.Code == FUTErrorCode.InvalidCookie)
                {
                    AddLog("Invalid Cookie relogging! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                    UpdateStatistic("Invalid Cookie relogging! Request: " + futResponse.GetType().Name + " Last: " + _lastRequest?.GetType().Name);
                    FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Invalid Cookie relogging!");
                    if (_invalidCookieErrorCollection.IncrementAndCheck())
                    {
                        AddLog("Invalid Cookie Error occured 2 times in 5 minutes!!! Pausing Logic for 1 hour!");
                        UpdateStatistic("Invalid Cookie Error occured 2 times in 5 minutes!!! Pausing Logic for 1 hour!");
                        FUTLogsDatabase.AddFUTNotification(FUTAccount.EMail, "Invalid Cookie Error occured 2 times in 5 minutes!!! Pausing Logic for 1 hour!");
                        PauseLogic(1);
                    }
                    CookieManager.DeleteCookieContainer(FUTAccount);
                    CookieManager.DeleteCookieContainer(FUTAccount, false);
                    ResetRequestFactory();
                }
                else
                {
                    if(futResponse.Code == FUTErrorCode.PermissionDenied && futResponse.GetType() != typeof(BuyItemResponse))
                    {
                        AddLog(futResponse.GetType().Name + " failed -> " + Enum.GetName(typeof(FUTErrorCode), futResponse.Code) + " (" + (int)futResponse.Code + ")");
                    }
                    
                }
            }
            return futResponse.HasError;
        }

    }
}
