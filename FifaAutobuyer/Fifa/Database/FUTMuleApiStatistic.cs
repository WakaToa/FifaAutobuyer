using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Managers;
using Newtonsoft.Json;

namespace FifaAutobuyer.Fifa.Database
{
    public class FUTMuleApiStatistic
    {
        private static FUTMuleApiStatistic _instance;
        public static FUTMuleApiStatistic Instance => _instance ?? (_instance = GetInstance());

        [Key]
        public int ID { get; set; }

        public double GTETotalDollarVolume { get; set; }
        public int GTETotalCoinVolume { get; set; }
        public double MFTotalDollarVolume { get; set; }
        public int MFTotalCoinVolume { get; set; }
        public double WSTotalDollarVolume { get; set; }
        public int WSTotalCoinVolume { get; set; }

        public void Reset()
        {
            GTETotalCoinVolume = 0;
            GTETotalDollarVolume = 0;
            MFTotalCoinVolume = 0;
            MFTotalDollarVolume = 0;
            WSTotalCoinVolume = 0;
            WSTotalDollarVolume = 0;
        }

        private static readonly object _statisticLock = new object();

        public void SaveChanges()
        {
            lock (_statisticLock)
            {
                using (var context = new FUTLogsDatabase())
                {
                    var statistic = context.FUTMuleApiStatistics.FirstOrDefault();
                    if (statistic == null)
                    {
                        statistic = new FUTMuleApiStatistic();
                        statistic.Reset();
                        context.FUTMuleApiStatistics.Add(statistic);
                        context.SaveChanges();
                    }
                    else
                    {
                        context.Entry(statistic).CurrentValues.SetValues(this);
                        context.SaveChanges();
                    }
                }
            }
        }

        public static FUTMuleApiStatistic GetInstance()
        {
            lock (_statisticLock)
            {
                using (var ctx = new FUTLogsDatabase())
                {
                    var statistic = ctx.FUTMuleApiStatistics.FirstOrDefault();
                    if (statistic == null)
                    {
                        var ret = new FUTMuleApiStatistic();
                        ret.Reset();
                        ctx.FUTMuleApiStatistics.Add(ret);
                        ctx.SaveChanges();
                        return ret;
                    }
                    return statistic;
                }
            }
        }

    }
}
