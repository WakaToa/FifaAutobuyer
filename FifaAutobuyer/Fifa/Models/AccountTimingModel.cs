using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class AccountTimingModel
    {
        public SimpleTime AccountDayStart { get; set; }

        public SimpleTime AccountFirstSmallPause { get; set; }
        public SimpleTime AccountSecondSmallPause { get; set; }

        public SimpleTime AccountFirstBigPause { get; set; }

        public SimpleTime AccountThirdSmallPause { get; set; }
        public SimpleTime AccountFourthSmallPause { get; set; }

        public SimpleTime AccountSecondBigPause { get; set; }

        public SimpleTime AccountFifthSmallPause { get; set; }
        public SimpleTime AccountSixthSmallPause { get; set; }

        public SimpleTime AccountDayEnd { get; set; }

        public AccountTimingModel(Random random)
        {
            AccountDayStart = new SimpleTime();
            AccountFirstSmallPause = new SimpleTime();
            AccountSecondSmallPause = new SimpleTime();
            AccountFirstBigPause = new SimpleTime();
            AccountThirdSmallPause = new SimpleTime();
            AccountFourthSmallPause = new SimpleTime();
            AccountSecondBigPause = new SimpleTime();
            AccountFifthSmallPause = new SimpleTime();
            AccountSixthSmallPause = new SimpleTime();
            AccountDayEnd = new SimpleTime();

            AccountDayStart.Hour = random.Next(9, 11);
            AccountDayStart.Minute = random.Next(0, 59);
            if(AccountDayStart.Exceeds(DateTime.Now))
            {
                AccountDayStart.Finished = true;
            }

            AccountFirstSmallPause.Hour = AccountDayStart.Hour + 1;
            AccountFirstSmallPause.Minute = random.Next(0, 59);
            if (AccountFirstSmallPause.Exceeds(DateTime.Now))
            {
                AccountFirstSmallPause.Finished = true;
            }

            AccountSecondSmallPause.Hour = AccountFirstSmallPause.Hour + 1;
            AccountSecondSmallPause.Minute = random.Next(0, 59);
            if (AccountSecondSmallPause.Exceeds(DateTime.Now))
            {
                AccountSecondSmallPause.Finished = true;
            }

            AccountFirstBigPause.Hour = AccountDayStart.Hour + 5;
            AccountFirstBigPause.Minute = random.Next(0, 59);
            if (AccountFirstBigPause.Exceeds(DateTime.Now))
            {
                AccountFirstBigPause.Finished = true;
            }

            AccountThirdSmallPause.Hour = AccountFirstBigPause.Hour + 1;
            AccountThirdSmallPause.Minute = random.Next(0, 59);
            if (AccountThirdSmallPause.Exceeds(DateTime.Now))
            {
                AccountThirdSmallPause.Finished = true;
            }

            AccountFourthSmallPause.Hour = AccountFourthSmallPause.Hour + 1;
            AccountFourthSmallPause.Minute = random.Next(0, 59);
            if (AccountFourthSmallPause.Exceeds(DateTime.Now))
            {
                AccountFourthSmallPause.Finished = true;
            }

            AccountSecondBigPause.Hour = AccountFirstBigPause.Hour + 5;
            AccountSecondBigPause.Minute = random.Next(0, 59);
            if (AccountSecondBigPause.Exceeds(DateTime.Now))
            {
                AccountSecondBigPause.Finished = true;
            }

            AccountFifthSmallPause.Hour = AccountSecondBigPause.Hour + 1;
            AccountFifthSmallPause.Minute = random.Next(0, 59);
            if (AccountFifthSmallPause.Exceeds(DateTime.Now))
            {
                AccountFifthSmallPause.Finished = true;
            }

            AccountSixthSmallPause.Hour = AccountFifthSmallPause.Hour + 1;
            AccountSixthSmallPause.Minute = random.Next(0, 59);
            if (AccountSixthSmallPause.Exceeds(DateTime.Now))
            {
                AccountSixthSmallPause.Finished = true;
            }

            AccountDayEnd.Hour = AccountSecondBigPause.Hour + 5;
            AccountDayEnd.Minute = random.Next(0, 59);
        }
    }
}
