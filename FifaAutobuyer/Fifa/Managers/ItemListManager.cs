using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.Fifa.Managers
{
    public class ItemListManager
    {
        private static int _counterNextItem = 0;
        public static FUTListItem GetNextFreeItem()
        {
            var items = GetFUTListItems().Where(x => x.BuyItem);
            var futListItems = items as FUTListItem[] ?? items.ToArray();
            return !futListItems.Any() ? null : futListItems.PickRandom();
        }

        private static object _lockObjectPriceCheck = new object();
        public static FUTListItem GetNextPriceCheckItem()
        {
            lock (_lockObjectPriceCheck)
            {
                var items = GetFUTListItems();
                if (items == null || items.Count == 0)
                {
                    return null;
                }
                foreach (var currentItem in items)
                {
                    if(currentItem == null)
                    {
                        continue;
                    }
                    if (!currentItem.IgnorePriceCheck && !currentItem.PriceChecking && DateTime.Now.Subtract(Helper.TimestampToDateTime(currentItem.LastPriceCheck)).TotalMinutes >= FUTSettings.Instance.PriceCheckTimes)
                    {
                        currentItem.PriceChecking = true;
                        currentItem.SaveChanges();
                        return currentItem;
                    }
                }
                return null;
            }
        }

        public static List<FUTListItem> GetFUTListItems()
        {
            using (var context = new FUTSettingsDatabase())
            {
                return context.FUTListItems.ToList();
            }
        }

        public static FUTListItem GetMostMatchingListItem(int assetID, int revID, int style, string pos)
        {
            FUTListItem listItem = ItemListManager.GetFUTListItems().FirstOrDefault(x => x.AssetID == assetID && x.RevisionID == revID);

            var specific1 = ItemListManager.GetFUTListItems()
                .FirstOrDefault(x => x.AssetID == assetID && x.RevisionID == revID && pos == x.Position);
            if (specific1 != null)
            {
                listItem = specific1;
            }
            var specific2 = ItemListManager.GetFUTListItems()
                .FirstOrDefault(x => x.AssetID == assetID && x.RevisionID == revID &&
                                     style == (int)x.ChemistryStyle);
            if (specific2 != null)
            {
                listItem = specific2;
            }
            var specific3 = ItemListManager.GetFUTListItems()
                .FirstOrDefault(x => x.AssetID == assetID && x.RevisionID == revID &&
                                     style == (int)x.ChemistryStyle && pos == x.Position);
            if (specific3 != null)
            {
                listItem = specific3;
            }
            return listItem;
        }

        public static bool ItemExistsInList(int assetID, int revID, string position, ChemistryStyle style)
        {
            return GetFUTListItems().Any(x => x.AssetID == assetID && x.RevisionID == revID && position == x.Position && style == x.ChemistryStyle);
        }

        public static void InsertFUTListItem(FUTListItem item)
        {
            using (var context = new FUTSettingsDatabase())
            {
                context.FUTListItems.Add(item);
                context.SaveChanges();
            }
        }

        public static void UpdateFUTListItem(FUTListItem item)
        {
            using (var context = new FUTSettingsDatabase())
            {
                var savedItem = context.FUTListItems.FirstOrDefault(x => x.ID == item.ID);
                if(savedItem == null)
                {
                    InsertFUTListItem(item);
                }
                else
                {
                    context.Entry(savedItem).CurrentValues.SetValues(item);
                    context.SaveChanges();
                }
            }
        }

        public static void RemoveFUTListItem(int databaseID)
        {
            using (var context = new FUTSettingsDatabase())
            {
                var item = context.FUTListItems.FirstOrDefault(x => x.ID == databaseID);
                if(item != null)
                {
                    context.FUTListItems.Remove(item);
                    context.SaveChanges();
                }
            }
        }

        public static void AdjustVariableBuyPercent(FUTListItem listItem)
        {
            using (var context = new FUTSettingsDatabase())
            {
                var item = context.FUTListItems.FirstOrDefault(x => x.ID == listItem.ID);
                if (item != null)
                {
                    item.VariableBuyPercent = item.StaticBuyPercent - item.BuyPercentStep;
                    context.SaveChanges();
                }
            }
        }

        public static void ResetPriceCheckEverywhere()
        {
            var items = GetFUTListItems();
            foreach (var item in items)
            {
                try
                {
                    if(!item.IgnorePriceCheck)
                    {
                        item.LastPriceCheck = 0;
                        item.PriceChecking = false;
                        item.BuyPrice = 0;
                        item.SellPrice = 0;
                        item.SaveChanges();
                    }
                    
                }
                catch { }
            }
        }
    }
}
