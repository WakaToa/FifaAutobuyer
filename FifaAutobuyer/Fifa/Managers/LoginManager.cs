using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Semaphore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.Fifa.Managers
{
    public class LoginManager
    {
        private static Dictionary<string, AsyncSemaphore> _loginSemaphores;

        private static object _lock = new object();

        public static AsyncSemaphore GetSemaphoreForProxy(FUTProxy proxy)
        {
            AsyncSemaphore returnType = null;
            lock(_lock)
            {
                if (_loginSemaphores == null)
                {
                    _loginSemaphores = new Dictionary<string, AsyncSemaphore>();
                }
                if (proxy == null)
                {
                    returnType = GetSemaphoreForExe();
                }
                else
                {
                    if (!_loginSemaphores.ContainsKey(proxy.ToString()))
                    {
                        _loginSemaphores.Add(proxy.ToString(), new AsyncSemaphore(1));
                    }
                    returnType = _loginSemaphores[proxy.ToString()];
                }
            }
            return returnType;
        }

        public static AsyncSemaphore GetSemaphoreForExe()
        {
            AsyncSemaphore returnType = null;
            lock (_lock)
            {
                if (_loginSemaphores == null)
                {
                    _loginSemaphores = new Dictionary<string, AsyncSemaphore>();
                }
                if (!_loginSemaphores.ContainsKey("EXE"))
                {
                    _loginSemaphores.Add("EXE", new AsyncSemaphore(1));
                }

                returnType = _loginSemaphores["EXE"];
            }
            return returnType;
        }
    }
}
