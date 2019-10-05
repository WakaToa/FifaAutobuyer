using FifaAutobuyer.Database.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Http
{
    public class RequestPerMinuteManager
    {
        private DateTime _lastRequest = DateTime.Now;

        private object _lockObject = new object();

        private Random _random = new Random();

        private int _calculatedTime = -1;
        public async Task WaitForNextRequest()
        {
            await Task.Run(() =>
            {
                WaitForNextRequestInternal();
            }).ConfigureAwait(false);
        }

        public bool IsNextRequestPossible()
        {
            if (_calculatedTime < 0)
            {
                var timeMin = 0;
                var timeMax = 0;
                if (_isSearch)
                {
                    timeMin = 60000 / FUTSettings.Instance.RoundsPerMinuteMinSearch;
                    timeMax = 60000 / FUTSettings.Instance.RoundsPerMinuteMaxSearch;
                }
                else
                {
                    timeMin = 60000 / FUTSettings.Instance.RoundsPerMinuteMin;
                    timeMax = 60000 / FUTSettings.Instance.RoundsPerMinuteMax;
                }

                _calculatedTime = _random.Next(timeMax, timeMin);
            }

            var result =  (_lastRequest.AddMilliseconds(_calculatedTime) <= DateTime.Now);
            return result;
        }

        public RequestPerMinuteManager(bool isSearch = false)
        {
            _isSearch = isSearch;
        }
        private readonly bool _isSearch = false;

        private void WaitForNextRequestInternal()
        {
            lock (_lockObject)
            {
                while (!IsNextRequestPossible())
                {
                    Thread.Sleep(10);
                }
                _lastRequest = DateTime.Now;
                _calculatedTime = -1;
            }
        }
    }
}
