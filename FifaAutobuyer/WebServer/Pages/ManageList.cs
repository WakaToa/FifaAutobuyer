using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Newtonsoft.Json;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class ManageList : NancyModule
    {
        public ManageList()
        {
            this.RequiresAuthentication();
            Get("/managelist", args =>
            {
                var model = new ManageListModel();


                var itemsInList = ItemListManager.GetFUTListItems();

                model.FutListItems = itemsInList;
                return View["ManageList", model];
            });

            Post("/resetpricecheck", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var databaseID = int.Parse(parameters["item"]);

                var itemsMatching = ItemListManager.GetFUTListItems().FirstOrDefault(x => x.ID == databaseID);
                if (itemsMatching != null)
                {
                    itemsMatching.LastPriceCheck = 0;
                    itemsMatching.PriceChecking = false;
                    itemsMatching.SaveChanges();
                }

                var itemsMatchingNew = ItemListManager.GetFUTListItems().FirstOrDefault(x => x.ID == databaseID);
                var itemsJson = JsonConvert.SerializeObject(itemsMatchingNew);

                return itemsJson;
            });

            Post("/removeitemfromlist", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var databaseId = int.Parse(parameters["item"]);

                ItemListManager.RemoveFUTListItem(databaseId);

                return Response.AsRedirect("/managelist");
            });

            Post("/saveitem", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);


                var databaseId = int.Parse(parameters["itemDatabaseID"]);

                var item = ItemListManager.GetFUTListItems().FirstOrDefault(x => x.ID == databaseId);
                if (item != null)
                {
                    item.StaticBuyPercent = int.Parse(parameters["staticBuyPercent"]);
                    item.VariableBuyPercent = int.Parse(parameters["variableBuyPercent"]);
                    item.BuyPercentStep = int.Parse(parameters["buyPercentStep"]);
                    item.SellPercent = int.Parse(parameters["sellPercent"]);
                    item.Counter = int.Parse(parameters["counter"]);
                    item.BuyPrice = int.Parse(parameters["buyPrice"]);
                    item.SellPrice = int.Parse(parameters["sellPrice"]);
                    item.IgnorePriceCheck = parameters["ignorePriceCheck"] != "false";
                    item.BuyItem = parameters["buyItem"] != "false";
                    item.Discard = parameters["discardItem"] != "false";
                    item.SaveChanges();
                }


                var itemJson = JsonConvert.SerializeObject(item);

                return itemJson;
            });

            Post("/searchforitem", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var item = parameters["item"];

                var items = FUTItemManager.GetMatchingItems(item);

                var userFriendlyItems = items.Select(p => new UserFriendlyItemObject
                {
                    AssetID = p.id, Rating = p.r, Name = p.GetName(), RevisionID = p.RevisionID, Type = p.Type
                }).ToList();

                var itemsJson = JsonConvert.SerializeObject(userFriendlyItems);

                return itemsJson;
            });

            Post("/additem", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var assetId = int.Parse(parameters["addAssetID"]);
                var revId = int.Parse(parameters["revID"]);
                var itemType = int.Parse(parameters["itemType"]);
                var position = parameters["pos"];
                var playStyle = (ChemistryStyle)int.Parse(parameters["playStyle"]);

                if (ItemListManager.ItemExistsInList(assetId, revId, position, playStyle))
                {
                    return Response.AsRedirect("/managelist?error=1");
                }

                var newPlayerObject = new FUTListItem(assetId)
                {
                    RevisionID = revId,
                    Type = (FUTSearchParameterType) itemType,
                    BuyPercentStep = 0,
                    StaticBuyPercent = FUTSettings.Instance.BuyPercent,
                    VariableBuyPercent = FUTSettings.Instance.BuyPercent,
                    SellPercent = FUTSettings.Instance.SellPercent,
                    Counter = 10,
                    BuyItem = false,
                    IgnorePriceCheck = true,
                    Position = position,
                    ChemistryStyle = playStyle
                };

                ItemListManager.InsertFUTListItem(newPlayerObject);

                return Response.AsRedirect("/managelist");
            });
        }
    }
}
