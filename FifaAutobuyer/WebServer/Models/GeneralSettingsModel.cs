using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.WebServer.Models
{
    class GeneralSettingsModel : BaseModel
    {
        public int RoundsPerMinuteMax => FUTSettings.Instance.RoundsPerMinuteMax;
        public int RoundsPerMinuteMin => FUTSettings.Instance.RoundsPerMinuteMin;
        public int RoundsPerMinuteMaxSearch => FUTSettings.Instance.RoundsPerMinuteMaxSearch;
        public int RoundsPerMinuteMinSearch => FUTSettings.Instance.RoundsPerMinuteMinSearch;
        public int TradepileCheck => FUTSettings.Instance.TradepileCheck;
        public int PauseBetweenRelogs => FUTSettings.Instance.PauseBetweenRelogs;
        public int Counter => FUTSettings.Instance.Counter;
        public int PriceCheckTimes => FUTSettings.Instance.PriceCheckTimes;
        public bool EnableBuy => FUTSettings.Instance.EnableBuy;
        public bool EnableSell => FUTSettings.Instance.EnableSell;
        public string EnableBuyChecked => EnableBuy ? "checked" : "";
        public string EnableSellChecked => EnableSell ? "checked" : "";
        public bool RelistWithOldPrice => FUTSettings.Instance.RelistWithOldPrice;
        public string RelistWithOldPriceChecked => RelistWithOldPrice ? "checked" : "";
        public int UseLastPriceChecks => FUTSettings.Instance.UseLastPriceChecks;
        public bool DiscardEverything => FUTSettings.Instance.DiscardEverything;
        public string DiscardEverythingChecked => DiscardEverything ? "checked" : "";
        public LoginMethod LoginMethod => FUTSettings.Instance.LoginMethod;
        public bool LoginMethodWebEnabled => FUTSettings.Instance.LoginMethod == LoginMethod.Web;
        public bool LoginMethodIOSEnabled => FUTSettings.Instance.LoginMethod == LoginMethod.IOS;
        public bool LoginMethodAndroidEnabled => FUTSettings.Instance.LoginMethod == LoginMethod.Android;
        public int PriceCorrectionPercentage => FUTSettings.Instance.PriceCorrectionPercentage;
        public bool UseBidSwitch => FUTSettings.Instance.UseBidSwitch;
        public string UseBidSwitchChecked => UseBidSwitch ? "checked" : "";
        public bool UseRandomRequests => FUTSettings.Instance.UseRandomRequests;
        public string UseRandomRequestsChecked => UseRandomRequests ? "checked" : "";
        public int WatchlistCheck => FUTSettings.Instance.WatchlistCheck;
        public int ExpiredTimer => FUTSettings.Instance.ExpiredTimer;
        public bool OneParallelLogin => FUTSettings.Instance.OneParallelLogin;
        public string OneParallelLoginChecked => OneParallelLogin ? "checked" : "";
        public int WaitAfterBuy => FUTSettings.Instance.WaitAfterBuy;
        public int MaxCardsPerDay => FUTSettings.Instance.MaxCardsPerDay;
        public int MinimumPlayersForPriceCheck => FUTSettings.Instance.MinimumPlayersForPriceCheck;

        public GeneralSettingsModel()
        {
            GeneralSettingsActive = "active";
        }
    }
}
