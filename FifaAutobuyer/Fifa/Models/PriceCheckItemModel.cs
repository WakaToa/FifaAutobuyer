using FifaAutobuyer.Fifa.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Services;

namespace FifaAutobuyer.Fifa.Models
{
    public class PriceCheckItemModel : FUTSearchParameter
    {
        public int AssetID { get; set; }
        public int RevisionID { get; set; }

        public ChemistryStyle ChemistryStyle { get; set; }
        public string Position { get; set; }

        public int MinBuyNow { get; set; }

        public int BuyPrice { get; set; }

        private string RandomParameters()
        {
            var playerInfo = FUTItemManager.GetItemByAssetRevisionID(AssetID, RevisionID);

            var parameters = new List<string>();
            parameters.Add("&lev=" + (playerInfo.Rating < 65 ? "bronze" : playerInfo.Rating < 75 ? "silver" : "gold"));
            parameters.Add("&leag=" + playerInfo.LeagueID);
            parameters.Add("&team=" + playerInfo.ClubID);
            parameters.Add("&nat=" + playerInfo.NationID);
            var totalcount = Helper.RandomInt(1, 5);

            var randomRet = "";

            for (var i = 0; i < totalcount; i++)
            {
                var rnd = parameters.PickRandom();
                parameters.Remove(rnd);
                randomRet += rnd;
            }
            return randomRet;
        }

        public override string BuildUriString()
        {
            var str = "";
            if (Type == FUTSearchParameterType.Player)
            {
                str += "?type=player";
            }
            else if (Type == FUTSearchParameterType.Contract)
            {
                str += "?type=development";
            }
            else if (Type == FUTSearchParameterType.Fitness)
            {
                str += "?type=development";
            }
            else if (Type == FUTSearchParameterType.Healing)
            {
                str += "?type=development";
            }
            else if (Type == FUTSearchParameterType.Training)
            {
                str += "?type=training";
            }
            else if (Type == FUTSearchParameterType.Manager)
            {
                str += "?type=staff";
            }
            else if (Type == FUTSearchParameterType.Kit)
            {
                str += "?type=clubInfo";
            }
            else if (Type == FUTSearchParameterType.Badge)
            {
                str += "?type=clubInfo";
            }
            else if (Type == FUTSearchParameterType.Stadium)
            {
                str += "?type=stadium";
            }
            else if (Type == FUTSearchParameterType.Development)
            {
                str += "?type=development";
            }
            else
            {
                str += "?type=";
            }

            if (ChemistryStyle != ChemistryStyle.All)
            {
                str += "&playStyle=" + (int)ChemistryStyle;
            }
            if (Position != Models.Position.Any)
            {
                str += "&pos=" + Position;
            }


            if (AssetID > 0)
            {
                var maskedDefId = ResourceIDManager.AssetIDToDefinitionID(AssetID, RevisionID);
                str += "&maskedDefId=" + maskedDefId;
            }
            if (MinBuyNow > 0)
            {
                str += "&minb=" + MinBuyNow;
            }
            if (BuyPrice > 0)
            {
                str += "&maxb=" + BuyPrice;
            }

            if (FUTSettings.Instance.UseRandomRequests)
            {
                var playerInfo = FUTItemManager.GetItemByAssetRevisionID(AssetID, RevisionID);
                if (playerInfo.Type == FUTSearchParameterType.Player)
                {
                    str += RandomParameters();
                }
            }


            return str;
        }
    }
}
