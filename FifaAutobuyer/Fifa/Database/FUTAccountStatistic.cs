using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Services;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futaccountstatistic")]
    public class FUTAccountStatistic
    {
        [Key]
        public int ID { get; set; }
        public string EMail { get; set; }
        public int Runtime { get; set; }
        public string LastActionData { get; set; }
        public long LastActionTimestamp { get; set; }
        public string LastActionTimeString => $"{Helper.TimestampToDateTime(LastActionTimestamp):d/M/yyyy HH:mm:ss}";
        public int CoinsStarted { get; set; }
        public int TotalLogins { get; set; }
        public int TradepileCount { get; set; }

        public void Load(string email)
        {
            using (var ctx = new FUTLogsDatabase())
            {
                var model = ctx.FUTAccountStatistics.FirstOrDefault(x => x.EMail.ToLower() == email.ToLower());
                if(model != null)
                {
                    Runtime = model.Runtime;
                    LastActionData = model.LastActionData;
                    LastActionTimestamp = model.LastActionTimestamp;
                    TotalLogins = model.TotalLogins;
                    TradepileCount = model.TradepileCount;
                }
            }
        }

        public void Update()
        {
            using (var ctx = new FUTLogsDatabase())
            {
                var model = ctx.FUTAccountStatistics.FirstOrDefault(x => x.EMail.ToLower() == EMail.ToLower());
                if (model != null)
                {
                    model.CoinsStarted = CoinsStarted;
                    model.LastActionData = LastActionData;
                    model.LastActionTimestamp = LastActionTimestamp;
                    model.Runtime = Runtime;
                    model.TotalLogins = TotalLogins;
                    model.TradepileCount = TradepileCount;
                    //ctx.Entry(model).CurrentValues.SetValues(this);
                }
                else
                {
                    ctx.FUTAccountStatistics.Add(this);
                }
                ctx.SaveChanges();

            }
        }
    }
}
