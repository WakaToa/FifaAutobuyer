using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Services
{
    public static class Helper
    {
        public static long CreateTimestamp()
        {
            var date1 = new DateTime(1970, 1, 1);
            var date2 = DateTime.Now;
            var ts = new TimeSpan(date2.Ticks - date1.Ticks);
            return (Convert.ToInt64(ts.TotalMilliseconds));
        }

        public static DateTime TimestampToDateTime(long seconds)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddMilliseconds(seconds);
            return dateTime;
        }
        public static DateTime ToDateTime(this long seconds)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddMilliseconds(seconds);
            return dateTime;
        }

        static Random _random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static int RandomInt(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
