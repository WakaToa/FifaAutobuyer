using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Collection
{
    public class ErrorCollection
    {
        private int _minuteLimit;
        private int _counterLimit;
        private CustomCollection<DateTime> _collection;
        public ErrorCollection(int counterLimit, int minuteLimit)
        {
            _counterLimit = counterLimit;
            _minuteLimit = minuteLimit;
            _collection = new CustomCollection<DateTime>(_counterLimit);
        }

        private void IncrementError()
        {
            _collection.Add(DateTime.Now);
        }
        public bool IncrementAndCheck()
        {
            IncrementError();
            if (_collection.Count < _counterLimit)
            {
                return false;
            }
            var difference = (_collection.LastOrDefault() - _collection.FirstOrDefault());
            if (difference.TotalMinutes <= _minuteLimit)
            {
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _collection.Clear();
        }
    }
}
