using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.ActionScheduler
{
    public enum ActionType
    {
        None,
        SwitchToWebEmulation,
        SwitchToIOSEmulation,
        SwitchToAndroidEmulation,
        StartLogic,
        StopLogic,
        EnableBuy,
        DisableBuy,
        EnableSell,
        DisableSell,
        SetBuyPercent,
        SetSellPercent,
        ResetPricechecks
    }

    public class ActionModel
    {
        public int ID { get; set; }
        public ActionType Type { get; set; }
        public TimeSpan Time { get; set; }
        public string Description { get; set; }
        public int Percent { get; set; }

        [JsonIgnore]
        public DateTime NextExecution => DateTime.Now.TimeOfDay <= Time ? DateTime.Today.Add(Time) : DateTime.Today.AddDays(1).Add(Time);
    }
}
