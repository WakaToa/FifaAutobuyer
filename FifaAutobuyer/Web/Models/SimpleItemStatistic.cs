using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Web.Models
{
    public class SimpleItemStatistic
    {
        public string Name { get; set; }
        public int AssetID { get; set; }
        public int RevisionID { get; set; }

        public int AverageBuyprice { get; set; }
        public int AverageSellprice { get; set; }
        public int AverageProfit { get; set; }
        public int TotalBuys { get; set; }
        public int TotalSells { get; set; }
        public int TotalProfit { get; set; }
        public int TotalBuyValue{ get; set; }
        public int TotalSellValue { get; set; }
        public int ExpectedProfit { get; set; }
        public int TradepileValue { get; set; }
        public int ItemsLeftOnTradepile { get; set; }

        public SimpleItemStatistic()
        {

        }

        public SimpleItemStatistic(List<FUTBuy> buys, List<FUTSell> sells)
        {
            Fill(buys, sells);
        }

        public SimpleItemStatistic(List<FUTItemProfit> itemProfits)
        {
            Fill(itemProfits);
        }

        private void Fill(List<FUTBuy> buys, List<FUTSell> sells)
        {
            TotalBuys = buys.Count;
            TotalSells = sells.Count;
            TotalBuyValue = buys.Sum(x => x.BuyNowPrice);
            TotalSellValue = sells.Sum(x => x.SellPrice);
            TotalProfit = (int)(TotalSellValue * 0.95) - TotalBuyValue;
            if (buys.Count != 0)
            {
                AverageBuyprice = (int)(TotalBuyValue / buys.Count);
            }
            if (sells.Count != 0)
            {
                AverageSellprice = (int)(TotalSellValue / sells.Count);
            }
            AverageProfit = AverageSellprice - AverageBuyprice;
            ExpectedProfit = (buys.Count * AverageProfit);
        }

        private void Fill(List<FUTItemProfit> itemProfits)
        {
            var buys = itemProfits;
            var sells = itemProfits.Where(x => x.SellTimestamp != 0).ToList();
            TotalBuys = buys.Count;
            TotalSells = sells.Count;
            TotalBuyValue = buys.Sum(x => x.BuyPrice);
            TotalSellValue = sells.Sum(x => x.SellPrice);
            TotalProfit = (int)(TotalSellValue * 0.95) - TotalBuyValue;
            if (buys.Count != 0)
            {
                AverageBuyprice = (int)(TotalBuyValue / buys.Count);
            }
            if (sells.Count != 0)
            {
                AverageSellprice = (int)(TotalSellValue / sells.Count);
            }
            AverageProfit = AverageSellprice - AverageBuyprice;
            ExpectedProfit = (buys.Count * AverageProfit);
        }

    }
}
