using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace FifaAutobuyer.Fifa.Managers
{
    public class FUTItemManager
    {
        //private static List<SimpleSearchItemModel> _playerItems;
        //private static WebClient _webClient = new WebClient() { Encoding = Encoding.UTF8 };

        //private static SimpleSearchItemModel GetWebPlayerItemByAssetID(int baseID)
        //{
        //    try
        //    {
        //        if (_playerItems == null)
        //        {
        //            var playerDataStr = _webClient.DownloadString("http://cdn.content.easports.com/fifa/fltOnlineAssets/B488919F-23B5-497F-9FC0-CACFB38863D0/2016/fut/items/web/players.json");
        //            playerDataStr = HttpUtility.HtmlDecode(playerDataStr);
        //            _playerItems = JsonConvert.DeserializeObject<PlayersJsonResponse>(playerDataStr).AllPlayers();
        //        }

        //        var player =  _playerItems.Where(x => x.id == baseID).FirstOrDefault();
        //        return player;
        //    }
        //    catch(Exception e)
        //    {
        //        return new SimpleSearchItemModel();
        //    }
        //}

        public static SimpleSearchItemModel GetItemByAssetRevisionID(int baseID, int revID)
        {
            lock(_xmlItemsLock)
            {
                if (_xmlItems == null)
                {
                    var items = File.ReadAllText("items.json");
                    _xmlItems = JsonConvert.DeserializeObject<List<SimpleSearchItemModel>>(items);
                }
                //var player = GetWebPlayerItemByAssetID(baseID);
                //if (player != null)
                //{
                //    return player;
                //}
                //var fitness = GetConsumableItemByAssetID(baseID);
                //if (fitness != null)
                //{
                //    return fitness;
                //}
                return GetMatchingItems(baseID, revID).FirstOrDefault();
            }
        }

        //private static List<SimpleSearchItemModel> _consumableItems;
        //public static SimpleSearchItemModel GetConsumableItemByAssetID(int baseID)
        //{
        //    try
        //    {
        //        if (_consumableItems == null)
        //        {
        //            _consumableItems = new List<SimpleSearchItemModel>();
        //            _consumableItems.Add(new SimpleSearchItemModel() { Type = FUTSearchParameterType.Fitness, c = "Fitness 30 Squad", id = 5002006, RevisionID = 0 });
        //            _consumableItems.Add(new SimpleSearchItemModel() { Type = FUTSearchParameterType.Fitness, c = "Fitness 60 Player", id = 5002003, RevisionID = 0 });
        //            _consumableItems.Add(new SimpleSearchItemModel() { Type = FUTSearchParameterType.Contract, c = "Contract +28", id = 5001006, RevisionID = 0 });
        //            _consumableItems.Add(new SimpleSearchItemModel() { Type = FUTSearchParameterType.Healing, c = "Healing +4 All", id = 5002030, RevisionID = 0 });
        //            _consumableItems.Add(new SimpleSearchItemModel() { Type = FUTSearchParameterType.Healing, c = "Training +10 All", id = 5003042, RevisionID = 0 });
        //        }

        //        var fitness =  _consumableItems.Where(x => x.id == baseID).FirstOrDefault();
        //        return fitness;
        //    }
        //    catch (Exception e)
        //    {
        //        return new SimpleSearchItemModel();
        //    }
        //}

        private static List<SimpleSearchItemModel> _xmlItems;
        private static object _xmlItemsLock = new object();
        public static List<SimpleSearchItemModel> GetMatchingItems(string match)
        {
            lock (_xmlItemsLock)
            {
                if (_xmlItems == null)
                {
                    var items = File.ReadAllText("items.json");
                    _xmlItems = JsonConvert.DeserializeObject<List<SimpleSearchItemModel>>(items);
                }

                var ret = new List<SimpleSearchItemModel>();

                foreach (var item in _xmlItems)
                {
                    if (!string.IsNullOrEmpty(item.f) && item.f.ToLower().Contains(match.ToLower()))
                    {
                        ret.Add(item);
                    }
                    else if (!string.IsNullOrEmpty(item.l) && item.l.ToLower().Contains(match.ToLower()))
                    {
                        ret.Add(item);
                    }
                    else if (!string.IsNullOrEmpty(item.c) && item.c.ToLower().Contains(match.ToLower()))
                    {
                        ret.Add(item);
                    }
                }

                return ret;
            } 
        }

        public static List<SimpleSearchItemModel> GetMatchingItems(int baseID, int revisionID)
        {
            lock(_xmlItemsLock)
            {
                if (_xmlItems == null)
                {
                    var items = File.ReadAllText("items.json");
                    _xmlItems = JsonConvert.DeserializeObject<List<SimpleSearchItemModel>>(items);
                }

                var ret = new List<SimpleSearchItemModel>();

                foreach (var item in _xmlItems)
                {
                    if (item.id == baseID && item.RevisionID == revisionID)
                    {
                        ret.Add(item);
                    }
                }

                return ret;
            }
        }

        public static FUTSearchParameterType GetFUTSearchParamaterType(int baseid, int revisionID)
        {
            var item = GetMatchingItems(baseid, revisionID).FirstOrDefault();
            if(item == null)
            {
                return FUTSearchParameterType.Player;
            }
            return item.Type;
        }

        public static void ResetItems()
        {
            lock(_xmlItemsLock)
            {
                _xmlItems = null;
            }
        }

        public static void LoadItems()
        {
            lock (_xmlItemsLock)
            {
                if (_xmlItems != null) return;
                var items = File.ReadAllText("items.json");
                _xmlItems = JsonConvert.DeserializeObject<List<SimpleSearchItemModel>>(items);
            }
        }

    }
}
