using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Managers;
using ActionModel = FifaAutobuyer.Fifa.ActionScheduler.ActionModel;

namespace FifaAutobuyer.WebServer.Models
{
    class ActionSchedulerModel : BaseModel
    {
        public List<ActionModel> ActionScheduler => FUTSettings.Instance.ActionScheduler.ToList();
        public ActionSchedulerModel()
        {
            ActionSchedulerActive = "active";
        }
    }
}
