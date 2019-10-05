using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Models;
using FluentScheduler;

namespace FifaAutobuyer.Fifa.ActionScheduler
{
    public class SwitchToWebEmulationJob : IJob
    {
        public void Execute()
        {
            FUTSettings.Instance.LoginMethod = LoginMethod.Web;
            FUTSettings.Instance.SaveChanges();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Loginmethod switched to Web");
        }
    }
    public class SwitchToIOSEmulationJob : IJob
    {
        public void Execute()
        {
            FUTSettings.Instance.LoginMethod = LoginMethod.IOS;
            FUTSettings.Instance.SaveChanges();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Loginmethod switched to IOS");
        }
    }
    public class SwitchToAndroidEmulationJob : IJob
    {
        public void Execute()
        {
            FUTSettings.Instance.LoginMethod = LoginMethod.Android;
            FUTSettings.Instance.SaveChanges();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Loginmethod switched to Android");
        }
    }
    public class StopLogicJob : IJob
    {
        public void Execute()
        {
            BotManager.StopAllBots();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Logic stopped");
        }
    }
    public class StartLogicJob : IJob
    {
        public void Execute()
        {
            BotManager.StartAllBots();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Logic started");
        }
    }
    public class EnableBuyJob : IJob
    {
        public void Execute()
        {
            FUTSettings.Instance.EnableBuy = true;
            FUTSettings.Instance.SaveChanges();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Buy enabled");
        }
    }
    public class DisableBuyJob : IJob
    {
        public void Execute()
        {
            FUTSettings.Instance.EnableBuy= false;
            FUTSettings.Instance.SaveChanges();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Buy disabled");
        }
    }
    public class EnableSellJob : IJob
    {
        public void Execute()
        {
            FUTSettings.Instance.EnableSell = true;
            FUTSettings.Instance.SaveChanges();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Sell enabled");
        }
    }
    public class DisableSellJob : IJob
    {
        public void Execute()
        {
            FUTSettings.Instance.EnableSell = false;
            FUTSettings.Instance.SaveChanges();
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Sell disabled");
        }
    }
    public class SetBuyPercentJob : IJob
    {
        public int Percentage { get; set; }

        public void Execute()
        {
            foreach (var x in ItemListManager.GetFUTListItems())
            {
                x.StaticBuyPercent = Percentage;
                x.VariableBuyPercent = Percentage;
                x.SaveChanges();
            }
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Buypercent set to " + Percentage);
        }
    }
    public class SetSellPercentJob : IJob
    {
        public int Percentage { get; set; }

        public void Execute()
        {
            foreach (var x in ItemListManager.GetFUTListItems())
            {
                x.SellPercent = Percentage;
                x.SaveChanges();
            }
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Buypercent set to " + Percentage);
        }
    }
    public class ResetPricechecksJob : IJob
    {

        public void Execute()
        {
            foreach (var x in ItemListManager.GetFUTListItems())
            {
                x.LastPriceCheck = 0;
                x.SaveChanges();
            }
            FUTLogsDatabase.AddFUTNotification("ActionScheduler", "Pricechecks resetted");
        }
    }

    public class ActionScheduler : Registry
    {
        public ActionScheduler()
        {
            foreach (var actionModel in FUTSettings.Instance.ActionScheduler)
            {
                switch (actionModel.Type)
                {
                    case ActionType.SwitchToWebEmulation:
                        JobManager.AddJob(new SwitchToWebEmulationJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.SwitchToIOSEmulation:
                        JobManager.AddJob(new SwitchToIOSEmulationJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.SwitchToAndroidEmulation:
                        JobManager.AddJob(new SwitchToAndroidEmulationJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.StartLogic:
                        JobManager.AddJob(new StartLogicJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.StopLogic:
                        JobManager.AddJob(new StopLogicJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.EnableBuy:
                        JobManager.AddJob(new EnableBuyJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.DisableBuy:
                        JobManager.AddJob(new DisableBuyJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.EnableSell:
                        JobManager.AddJob(new EnableSellJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.DisableSell:
                        JobManager.AddJob(new DisableSellJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.SetBuyPercent:
                        JobManager.AddJob(new SetBuyPercentJob(){Percentage = actionModel.Percent}, s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.SetSellPercent:
                        JobManager.AddJob(new SetSellPercentJob() { Percentage = actionModel.Percent }, s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                    case ActionType.ResetPricechecks:
                        JobManager.AddJob(new ResetPricechecksJob(), s => s.ToRunEvery(1).Days().At(actionModel.Time.Hours, actionModel.Time.Minutes));
                        break;
                }
            }
        }

        public static void CreateScheduler()
        {
            JobManager.Stop();
            JobManager.RemoveAllJobs();

            JobManager.Initialize(new ActionScheduler());

            JobManager.AddJob(BotManager.ResetMuledCoins, schedule => schedule.ToRunEvery(1).Days().At(0,0));
            JobManager.AddJob(AuctionManager.RemoveOldAuctions, schedule => schedule.ToRunEvery(1).Hours());
            JobManager.AddJob(DatabaseScheduler.SaveBotStatistics, schedule => schedule.ToRunEvery(30).Minutes());
            JobManager.AddJob(DatabaseScheduler.DeleteOldLogs, schedule => schedule.ToRunEvery(30).Minutes());

            JobManager.Start();
        }
    }
}
