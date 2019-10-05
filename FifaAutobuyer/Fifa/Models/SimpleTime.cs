using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class SimpleTime
    {
        private int _hour;
        public int Hour
        {
            get { return _hour; }
            set
            {
                if (value >= 24)
                {
                    _hour = value - 24;
                }
                else if (value < 0)
                {
                    _hour = value + 24;
                }
                else
                {
                    _hour = value;
                }
            }
        }

        private int _minute;
        public int Minute
        {
            get { return _minute; }
            set
            {
                if (value >= 60)
                {
                    _minute = value - 60;
                }
                else if (value < 0)
                {
                    _minute = value + 60;
                }
                else
                {
                    _minute = value;
                }
            }
        }

        public bool Finished { get; set; }

        public bool Exceeds(DateTime datetime)
        {
            if(datetime.Hour < _hour)
            {
                return false;
            }
            if(datetime.Hour > _hour)
            {
                return true;
            }
            if(datetime.Hour == _hour && datetime.Minute >= _minute)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            var str = "";
            if(_hour.ToString().Length == 1)
            {
                str += "0" + _hour.ToString();
            }
            else
            {
                str += _hour.ToString();
            }
            str += ":";
            if (_minute.ToString().Length == 1)
            {
                str += "0" + _minute.ToString();
            }
            else
            {
                str += _minute.ToString();
            }
            return str;
        }

        public SimpleTime()
        {
            Finished = false;
        }
    }
}
