using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;

namespace FifaAutobuyer.Fifa.Managers
{
    public class ProxyManager
    {
        private static List<Pair<FUTProxy, int>> _futProxies;
        private static int _maxCounter = 12;
        private static object _locker = new object();
        public static void Initialize()
        {
            using (var ctx = new FUTSettingsDatabase())
            {
                var proxies = ctx.FUTProxys.ToList();
                _futProxies = new List<Pair<FUTProxy, int>>();
                foreach (var futProxy in proxies)
                {
                    _futProxies.Add(new Pair<FUTProxy, int>(futProxy, 0));
                }
            }
        }

        public static FUTProxy GetFUTProxy(FUTProxy pre)
        {
            lock (_locker)
            {
                if (pre != null)
                {
                    var preFound = _futProxies.FirstOrDefault(x => x.First.ToString() == pre.ToString());
                    if (preFound != null)
                    {
                        preFound.Second++;
                        return preFound.First;
                    }
                }

                foreach (var futProxy in _futProxies)
                {
                    if (futProxy.Second < _maxCounter)
                    {
                        futProxy.Second++;
                        return futProxy.First;
                    }
                }
                return null;
            }
        }

        public static void AddFUTProxy(FUTProxy proxy)
        {
            lock (_locker)
            {
                _futProxies.Add(new Pair<FUTProxy, int>(proxy, 0));
            }
        }

        public static void RemoveFUTProxy(FUTProxy proxy)
        {
            lock (_locker)
            {
                _futProxies.RemoveAll(x => x.First.ToString() == proxy.ToString());
            }
        }

        public static void RemoveProxyCounter(FUTProxy proxy)
        {
            lock (_locker)
            {
                if (proxy == null)
                {
                    return;
                }
                var p = _futProxies.FirstOrDefault(x => x.First == proxy);
                if (p != null)
                {
                    p.Second--;
                }
            }
        }

        public static void ResetProxyCounter()
        {
            lock (_locker)
            {
                using (var ctx = new FUTSettingsDatabase())
                {
                    var proxies = ctx.FUTProxys.ToList();
                    _futProxies = new List<Pair<FUTProxy, int>>();
                    foreach (var futProxy in proxies)
                    {
                        _futProxies.Add(new Pair<FUTProxy, int>(futProxy, 0));
                    }
                }
            }
        }
    }
}
