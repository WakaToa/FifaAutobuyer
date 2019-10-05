using FifaAutobuyer.Fifa.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.WebSockets;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.MuleApi.Clients;
using ActionModel = FifaAutobuyer.Fifa.ActionScheduler.ActionModel;

namespace FifaAutobuyer.Database.Settings
{
    public class FUTSettings
    {
        private static FUTSettings _instance;
        public static FUTSettings Instance => _instance ?? (_instance = GetInstance());

        [Key]
        public int ID { get; set; }

        public string GTEPartnerID { get; set; }
        public string GTEPartnerApiKey { get; set; }
        public string GTEClientPlatform { get; set; }

        public string MuleFactoryUser { get; set; }
        public string MuleFactorySecretWord { get; set; }
        public string MuleFactoryClientPlatform { get; set; }

        public string WholeSaleApiKey { get; set; }
        public string WholeSaleClientPlatform { get; set; }

        public int MuleApiMaxSellPerDayPerAccount { get; set; }
        public int MuleApiMaxTransactionValue { get; set; }
        public int MuleApiMinCoinsOnAccount { get; set; }
        public int MuleApiRequestDelay { get; set; }
        public int MuleApiSellDelayPerAccount { get; set; }


        public int RoundsPerMinuteMax { get; set; }
        public int RoundsPerMinuteMin { get; set; }
        public int RoundsPerMinuteMaxSearch { get; set; }
        public int RoundsPerMinuteMinSearch { get; set; }
        public int MinimumPlayersForPriceCheck { get; set; }
        public int TradepileCheck { get; set; }
        public int PauseBetweenRelogs { get; set; }
        public int BuyPercent { get; set; }
        public int SellPercent { get; set; }
        public int Counter { get; set; }
        public int PriceCheckTimes { get; set; }
        public bool EnableBuy { get; set; }
        public bool EnableSell { get; set; }
        public bool RelistWithOldPrice { get; set; }
        public int UseLastPriceChecks { get; set; }
        public bool DiscardEverything { get; set; }
        public LoginMethod LoginMethod { get; set; }
        public int PriceCorrectionPercentage { get; set; }
        public bool UseBidSwitch { get; set; }
        public bool UseRandomRequests { get; set; }
        public int WatchlistCheck { get; set; }
        public int ExpiredTimer { get; set; }
        public bool OneParallelLogin { get; set; }

        public int WaitAfterBuy { get; set; }
        public int MaxCardsPerDay { get; set; }

        public string TimeScheudulerPausesJson
        {
            get => TimeScheudulerPauses == null || !TimeScheudulerPauses.Any()
                ? null
                : JsonConvert.SerializeObject(TimeScheudulerPauses);

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    TimeScheudulerPauses = new List<TimeSchedulerModel>();
                else
                    TimeScheudulerPauses = JsonConvert.DeserializeObject<List<TimeSchedulerModel>>(value);
            }
        }

        [NotMapped]
        public List<TimeSchedulerModel> TimeScheudulerPauses { get; set; }

        public string ActionSchedulerJson
        {
            get => ActionScheduler == null || !ActionScheduler.Any()
                ? null
                : JsonConvert.SerializeObject(ActionScheduler);

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    ActionScheduler = new List<ActionModel>();
                else
                    ActionScheduler = JsonConvert.DeserializeObject<List<ActionModel>>(value);
            }
        }

        [NotMapped]
        public List<ActionModel> ActionScheduler { get; set; }

        public void Reset()
        {
            ID = 1;
            
            TimeScheudulerPauses = new List<TimeSchedulerModel>();
            TimeScheudulerPausesJson = string.Empty;


            ActionScheduler = new List<ActionModel>();
            ActionSchedulerJson = string.Empty;

            GTEPartnerID = string.Empty;
            GTEPartnerApiKey = string.Empty;
            GTEClientPlatform = GameTradeEasyApiClient.PlatformPS4;

            MuleFactoryUser = string.Empty;
            MuleFactorySecretWord = string.Empty;
            MuleFactoryClientPlatform = MuleFactoryApiClient.PlatformPS4;

            WholeSaleApiKey = string.Empty;
            WholeSaleClientPlatform = WholeSaleApiClient.PlatformPS4;

            MuleApiMaxSellPerDayPerAccount = 50000;
            MuleApiMaxTransactionValue = 50000;
            MuleApiMinCoinsOnAccount = 35000;
            MuleApiRequestDelay = 1000;
            MuleApiSellDelayPerAccount = 300;

            BuyPercent = 80;
            SellPercent = 99;

            RoundsPerMinuteMin = 4;
            RoundsPerMinuteMax = 5;
            RoundsPerMinuteMinSearch = 3;
            RoundsPerMinuteMaxSearch = 3;
            PauseBetweenRelogs = 20;
            PriceCheckTimes = 60;
            UseLastPriceChecks = 3;
            Counter = 3;
            EnableBuy = true;
            EnableSell = true;
            RelistWithOldPrice = true;
            DiscardEverything = false;
            UseBidSwitch = false;
            UseRandomRequests = false;
            WatchlistCheck = 180;
            ExpiredTimer = 60;
            TradepileCheck = 90;
            LoginMethod = LoginMethod.Web;
            OneParallelLogin = false;
            MinimumPlayersForPriceCheck = 15;

            WaitAfterBuy = 0;
            MaxCardsPerDay = 1000;
        }

        private static readonly object _settingsLock = new object();

        public void SaveChanges()
        {
            lock(_settingsLock)
            {
                using (var context = new FUTSettingsDatabase())
                {
                    var settings = context.FUTSettings.FirstOrDefault();
                    if (settings == null)
                    {
                        settings = new FUTSettings();
                        settings.Reset();
                        settings.TimeScheudulerPauses = new List<TimeSchedulerModel>();
                        settings.ActionScheduler = new List<ActionModel>();
                        context.FUTSettings.Add(settings);
                        context.SaveChanges();
                    }
                    else
                    {
                        context.Entry(settings).CurrentValues.SetValues(this);
                        context.SaveChanges();
                    }
                }
            }
        }

        public static FUTSettings GetInstance()
        {
            lock (_settingsLock)
            {
                using (var ctx = new FUTSettingsDatabase())
                {
                    var settings = ctx.FUTSettings.FirstOrDefault();
                    if (settings == null)
                    {
                        var ret = new FUTSettings();
                        ret.Reset();
                        ret.TimeScheudulerPauses = new List<TimeSchedulerModel>();
                        ret.ActionScheduler = new List<ActionModel>();
                        ctx.FUTSettings.Add(ret);
                        ctx.SaveChanges();
                        return ret;
                    }
                    return settings;
                }
            }
        }
    }
}
