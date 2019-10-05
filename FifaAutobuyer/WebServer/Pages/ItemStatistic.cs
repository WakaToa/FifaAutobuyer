using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class ItemStatistic : NancyModule
    {
        public ItemStatistic()
        {
            this.RequiresAuthentication();
            Get("/itemstatistic", args =>
            {
                var model = new ItemStatisticModel();
                model.ItemStatistic = new List<SimpleItemStatistic>();
                var listItems = ItemListManager.GetFUTListItems();
                var allProfits = FUTLogsDatabase.GetFUTProfitLogs();
                var allTpItems = Fifa.Managers.BotManager.GetTradepileItems();

                foreach (var futListItem in listItems)
                {
                    var profits = new List<FUTItemProfit>();
                    var tpValue = 0;
                    if (futListItem.ChemistryStyle == ChemistryStyle.All && futListItem.Position == Position.Any)
                    {
                        profits = allProfits.Where(x => x.AssetID == futListItem.AssetID && x.RevisionID == futListItem.RevisionID).ToList();
                        tpValue =
                            (int)allTpItems.Where(
                                    x =>
                                        ResourceIDManager.GetAssetID(x.itemData.resourceId) == futListItem.AssetID &&
                                        ResourceIDManager.GetRevID(x.itemData.resourceId) == futListItem.RevisionID)
                                .Sum(x => x.buyNowPrice * 0.95);
                    }
                    else if (futListItem.ChemistryStyle == ChemistryStyle.All && futListItem.Position != Position.Any)
                    {
                        profits = allProfits.Where(x => x.AssetID == futListItem.AssetID && x.RevisionID == futListItem.RevisionID && x.Position == futListItem.Position).ToList();
                        tpValue =
                            (int)allTpItems.Where(
                                    x =>
                                        ResourceIDManager.GetAssetID(x.itemData.resourceId) == futListItem.AssetID &&
                                        ResourceIDManager.GetRevID(x.itemData.resourceId) == futListItem.RevisionID && x.itemData.preferredPosition == futListItem.Position)
                                .Sum(x => x.buyNowPrice * 0.95);
                    }
                    else if (futListItem.ChemistryStyle != ChemistryStyle.All && futListItem.Position == Position.Any)
                    {
                        profits = allProfits.Where(x => x.AssetID == futListItem.AssetID && x.RevisionID == futListItem.RevisionID && x.ChemistryStyle == futListItem.ChemistryStyle).ToList();
                        tpValue =
                            (int)allTpItems.Where(
                                    x =>
                                        ResourceIDManager.GetAssetID(x.itemData.resourceId) == futListItem.AssetID &&
                                        ResourceIDManager.GetRevID(x.itemData.resourceId) == futListItem.RevisionID && x.itemData.playStyle == (int)futListItem.ChemistryStyle)
                                .Sum(x => x.buyNowPrice * 0.95);
                    }
                    else if (futListItem.ChemistryStyle != ChemistryStyle.All && futListItem.Position == Position.Any)
                    {
                        profits = allProfits.Where(x => x.AssetID == futListItem.AssetID && x.RevisionID == futListItem.RevisionID && x.ChemistryStyle == futListItem.ChemistryStyle && x.Position == futListItem.Position).ToList();
                        tpValue =
                            (int)allTpItems.Where(
                                    x =>
                                        ResourceIDManager.GetAssetID(x.itemData.resourceId) == futListItem.AssetID &&
                                        ResourceIDManager.GetRevID(x.itemData.resourceId) == futListItem.RevisionID && x.itemData.playStyle == (int)futListItem.ChemistryStyle && x.itemData.preferredPosition == futListItem.Position)
                                .Sum(x => x.buyNowPrice * 0.95);
                    }

                    var statistic = new SimpleItemStatistic(profits);
                    statistic.TradepileValue = tpValue;
                    statistic.ItemsLeftOnTradepile = profits.Count(x => x.SellTimestamp == 0);
                    statistic.Name = futListItem.DisplayName;
                    statistic.RevisionID = futListItem.RevisionID;
                    statistic.AssetID = futListItem.AssetID;
                    statistic.TotalSearched = futListItem.TimesSearched + 1;
                    statistic.RPMProfit = (int)(statistic.TotalProfit / (futListItem.TimesSearched + 1));

                    model.ItemStatistic.Add(statistic);
                }

                return View["ItemStatistic", model];
            });
        }
    }
}
