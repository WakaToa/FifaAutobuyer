using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Managers;

namespace FifaAutobuyer.WebServer.Models
{
    public class BaseModel
    {
        public string Platform => AppSettingsManager.GetInstanceDescription();

        public int NotificationCenterCount => Database.FUTLogsDatabase.GetFUTNotificationCount();
        public bool DisplayNotificationCenterCount => NotificationCenterCount > 0;

        public string HomeActive { get; set; } = "";
        public string NotificationCenterActive { get; set; } = "";
        public string BotManagerActive { get; set; } = "";
        public string GeneralSettingsActive { get; set; } = "";
        public string ManageListActive { get; set; } = "";
        public string AccountManagerActive { get; set; } = "";
        public string ProxyManagerActive { get; set; } = "";
        public string ActionSchedulerActive { get; set; } = "";

        public string ProfitLogsBINActive { get; set; } = "";
        public string BuysLogsBINActive { get; set; } = "";
        public string SellsLogsBINActive { get; set; } = "";
        public string ProfitLogsBIDActive { get; set; } = "";
        public string BuysLogsBIDActive { get; set; } = "";
        public string SellsLogsBIDActive { get; set; } = "";
        public string ProfitLogsCMBActive { get; set; } = "";
        public string BuysLogsCMBActive { get; set; } = "";
        public string SellsLogsCMBActive { get; set; } = "";
        public string BotLogsActive { get; set; } = "";
        public string ExceptionLogsActive { get; set; } = "";

        public string TreeviewBIDActive { get; set; } = "";
        public string TreeviewBINActive { get; set; } = "";
        public string TreeviewCMBActive { get; set; } = "";

        public string ItemStatisticActive { get; set; } = "";
        public string AccountStatisticActive { get; set; } = "";
        public string BotStatisticActive { get; set; } = "";

        public string MuleManagerActive { get; set; } = "";
        public string MuleApiManagerActive { get; set; } = "";

        public string JavaScript { get; set; } = "";
    }
}
