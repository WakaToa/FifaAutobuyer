using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Managers
{
    class MultipleTradesManager
    {
        private static readonly Queue<long> _queue;
        private static readonly int _maxCount;
        private static readonly object _syncRoot = new object();

        static MultipleTradesManager()
        {
            _maxCount = 100;
            _queue = new Queue<long>(_maxCount);
        }

        public static bool CheckIfTradeIsValid(long tradeID)
        {
            lock (_syncRoot)
            {
                if (_queue.Contains(tradeID))
                {
                    return false;
                }
                if (_queue.Count == _maxCount)
                {
                    _queue.Dequeue();
                }
                _queue.Enqueue(tradeID);
                return true;
            }
        }
    }
}
