using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.EADatabase
{
    public class EADatabaseScraper
    {
        private static readonly WebClient _webclient = new WebClient()
        {
            Encoding = Encoding.UTF8
        };

        private static async Task CreateJson()
        {
            if (!Directory.Exists("pages"))
            {
                Directory.CreateDirectory("pages");
            }
            var ret = new List<SimpleSearchItemModel>();
            var current = 1;
            var max = -1;
            while (true)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "pages\\" + current + ".json");

                if (!File.Exists(path))
                {
                    Console.WriteLine("Downloading page " + current + " from " + max);
                    await Task.Delay(1000);
                    var dl = await DownloadPageAsync(current);
                    File.WriteAllText(path, dl, Encoding.UTF8);
                }
                var txt = File.ReadAllText(path);
                var json = JsonConvert.DeserializeObject<EADatabaseResultJson>(txt);
                max = json.totalPages;
                foreach (var item in json.items)
                {
                    var obj = new SimpleSearchItemModel();
                    obj.Type = FUTSearchParameterType.Player;
                    obj.ClubID = item.club.id;
                    obj.LeagueID = item.league.id;
                    obj.NationID = item.nation.id;
                    obj.RareFlag = 0;
                    obj.Rating = item.rating;
                    var revID = ResourceIDManager.GetRevID(int.Parse(item.id));
                    var assetID = ResourceIDManager.GetAssetID(int.Parse(item.id));
                    obj.id = assetID;
                    obj.RevisionID = revID;
                    obj.c = item.commonName;
                    obj.f = item.firstName;
                    obj.l = item.lastName;
                    obj.n = item.nation.id;
                    obj.r = item.rating;
                    ret.Add(obj);
                }
                current++;
                if (current > max)
                {
                    break;
                }
            }

            File.WriteAllText("players.json", JsonConvert.SerializeObject(ret));
        }

        private static List<SimpleSearchItemModel> LoadPlayers()
        {
            var players = File.ReadAllText("players.json");
            return JsonConvert.DeserializeObject<List<SimpleSearchItemModel>>(players);
        }

        private static List<SimpleSearchItemModel> LoadConsumables()
        {
            if (!File.Exists("consumables.json"))
            {
                return new List<SimpleSearchItemModel>();
            }
            var consumables = File.ReadAllText("consumables.json");
            return JsonConvert.DeserializeObject<List<SimpleSearchItemModel>>(consumables);
        }


        public static async Task UpdateAsync()
        {
            await CreateJson();
            var obj = LoadPlayers();
            obj.AddRange(LoadConsumables());
            var json = JsonConvert.SerializeObject(obj);
            File.WriteAllText("items.json", json);
        }

        private static async Task<string> DownloadPageAsync(int page)
        {
            var txt = await
                _webclient.DownloadStringTaskAsync(
                    "https://www.easports.com/uk/fifa/ultimate-team/api/fut/item?page=" + page);
            return txt;
        }
    }
}
