using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;

namespace FifaAutobuyer.Fifa.Managers
{
    public class TimeSchedulerModel
    {
        public int ID { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
    public class TimeScheduler
    {
        public static Tuple<bool, TimeSpan> IsInPause()
        {
            var pauses = FUTSettings.Instance.TimeScheudulerPauses;
            var now = DateTime.Now;
            foreach (var timePause in pauses)
            {
                var startDate = DateTime.Today;
                var endDate = DateTime.Today;

                //Check whether the dalEnd is lesser than dalStart
                if (timePause.StartTime >= timePause.EndTime)
                {
                    //Increase the date if dalEnd is timespan of the Nextday 
                    endDate = endDate.AddDays(1);
                }

                //Assign the dalStart and dalEnd to the Dates
                startDate = startDate.Date + timePause.StartTime;
                endDate = endDate.Date + timePause.EndTime;

                if ((now >= startDate) && (now <= endDate))
                {
                    var diff = (endDate - now).Duration();
                    return new Tuple<bool, TimeSpan>(true, diff);
                }
            }
            return new Tuple<bool, TimeSpan>(false, TimeSpan.MinValue);
        }
    }
}
