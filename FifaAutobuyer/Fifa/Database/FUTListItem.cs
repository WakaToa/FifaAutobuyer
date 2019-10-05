using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futlistitems")]
    public class FUTListItem : FUTSearchParameter
    {
        [Key]
        public int ID { get; set; }

        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int StaticBuyPercent { get; set; }
        public int VariableBuyPercent { get; set; }
        public int BuyPercentStep { get; set; }
        public int SellPercent { get; set; }
        public int Counter { get; set; }
        public int MinimumMargin { get; set; }

        public int AssetID { get; set; }
        public int RevisionID { get; set; }
        public ChemistryStyle ChemistryStyle { get; set; }
        public string Position { get; set; }

        public long LastPriceCheck { get; set; }
        public bool PriceChecking { get; set; }
        public bool IgnorePriceCheck { get; set; }
        public string IgnorePriceCheckChecked => IgnorePriceCheck ? "checked" : "";
        public bool BuyItem { get; set; }
        public string BuyItemChecked => BuyItem ? "checked" : "";
        public bool Discard { get; set; }
        public string DiscardChecked => Discard ? "checked" : "";

        public int TimesSearched { get; set; }

        public void SaveChanges()
        {
            try
            {
                using (var context = new FUTSettingsDatabase())
                {
                    var me = context.FUTListItems.FirstOrDefault(x => x.ID == ID);
                    if (me != null)
                    {
                        context.Entry(me).CurrentValues.SetValues(this);
                        context.SaveChanges();
                    }
                }
            }
            catch { }
        }

        public FUTListItem()
        {
            BuyPrice = 0;
            SellPrice = 0;
            SellPercent = FUTSettings.Instance.SellPercent;
            StaticBuyPercent = FUTSettings.Instance.BuyPercent;
            VariableBuyPercent = FUTSettings.Instance.BuyPercent;
            BuyPercentStep = 5;
            Counter = 10;
            LastPriceCheck = 0;
            RevisionID = 0;
            PriceChecking = false;
            BuyItem = false;
            IgnorePriceCheck = true;
            Discard = false;

            TimesSearched = 0;

            ChemistryStyle = ChemistryStyle.All;
            Position = Models.Position.Any;
        }

        public FUTListItem(int assetID) : this()
        {
            AssetID = assetID;
        }

        public PriceCheckItemModel FormPriceCheckPlayer()
        {
            var ret = new PriceCheckItemModel();
            ret.AssetID = AssetID;
            ret.RevisionID = RevisionID;
            ret.Type = Type;
            ret.ChemistryStyle = ChemistryStyle;
            ret.Position = Position;
            return ret;
        }

        public TransferMarketSearchObject FormTransferMarketSearchObject()
        {
            var ret = new TransferMarketSearchObject();
            ret.AssetID = AssetID;
            ret.RevisionID = RevisionID;
            ret.Type = Type;
            ret.ChemistryStyle = ChemistryStyle;
            ret.Position = Position;
            return ret;
        }

        public string DisplayName
        {
            get
            {
                var item = FUTItemManager.GetItemByAssetRevisionID(AssetID, RevisionID);
                return item != null ? $"{item.GetName()} ({Rating} / {RevisionID} / {Position} / {ChemistryStyle})" : "";
            }
        }

        public int Rating
        {
            get
            {
                var item = FUTItemManager.GetItemByAssetRevisionID(AssetID, RevisionID);
                return item?.Rating ?? 0;
            }
        }

        public string LastPriceCheckString
        {
            get {
                var time = Helper.TimestampToDateTime(LastPriceCheck);
                return time.ToShortDateString() + " " + time.ToShortTimeString();
            }
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
            else if(Type == FUTSearchParameterType.Training)
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
            if (BuyPrice > 0)
            {
                str += "&maxb=" + BuyPrice;
            }
            return str;
        }
    }
}
