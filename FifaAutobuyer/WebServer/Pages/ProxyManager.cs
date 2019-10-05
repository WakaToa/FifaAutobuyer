using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class ProxyManager : NancyModule
    {
        public ProxyManager()
        {
            this.RequiresAuthentication();
            Get("/proxymanager", args =>
            {
                var mod = new ProxyManagerModel();

                var proxies = new List<FUTProxy>();
                using(var ctx = new FUTSettingsDatabase())
                {
                    proxies = ctx.FUTProxys.ToList();
                }
                mod.Proxies = proxies;
                return View["ProxyManager", mod];
            });

            Post("/proxymanager", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var proxies = parameters["proxies"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (var proxyString in proxies)
                {
                    if (proxyString.Split(':').Count() < 4)
                    {
                        continue;
                    }
                    var host = proxyString.Split(':')[0];
                    var port = int.Parse(proxyString.Split(':')[1]);
                    var username = proxyString.Split(':')[2];
                    var password = proxyString.Split(':')[3];
                    var futProxy = new FUTProxy();
                    futProxy.Host = host;
                    futProxy.Port = port;
                    futProxy.Username = username;
                    futProxy.Password = password;
                    using (var ctx = new FUTSettingsDatabase())
                    {
                        ctx.FUTProxys.Add(futProxy);
                        ctx.SaveChanges();
                    }
                    Fifa.Managers.ProxyManager.AddFUTProxy(futProxy);
                    
                }
                return Response.AsRedirect("/proxymanager");
            });

            Post("/deleteproxyarray", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var proxies = parameters["proxy"].Split(',');
                
                foreach (var p in proxies)
                {
                    FUTProxy proxy = null;
                    var id = int.Parse(p);
                    using (var ctx = new FUTSettingsDatabase())
                    {
                        proxy = ctx.FUTProxys.FirstOrDefault(x => x.ID == id);
                        if (proxy != null)
                        {
                            var accounts = FUTAccountsDatabase.GetFUTAccounts()
                                .Where(x => x.FUTProxyID == proxy.ID).ToList();
                            foreach (var futAccount in accounts)
                            {
                                futAccount.FUTProxyID = -1;
                                futAccount.SaveChanges();
                            }
                            ctx.FUTProxys.Remove(proxy);
                            ctx.SaveChanges();
                        }
                    }
                    Fifa.Managers.ProxyManager.RemoveFUTProxy(proxy);
                }
                return "true";
            });

            Post("/deleteproxy", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);
                var id = int.Parse(parameters["proxy"]);
                FUTProxy proxy = null;
                using (var ctx = new FUTSettingsDatabase())
                {
                    proxy = ctx.FUTProxys.FirstOrDefault(x => x.ID == id);
                    if (proxy != null)
                    {
                        var accounts = FUTAccountsDatabase.GetFUTAccounts()
                            .Where(x => x.FUTProxyID == proxy.ID).ToList();
                        foreach (var futAccount in accounts)
                        {
                            futAccount.FUTProxyID = -1;
                            futAccount.SaveChanges();
                        }
                        ctx.FUTProxys.Remove(proxy);
                        ctx.SaveChanges();
                    }
                }
                Fifa.Managers.ProxyManager.RemoveFUTProxy(proxy);
                return "true";
            });

            Post("/allocateproxies", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);
                var action = parameters["action"];
                if (action == "allocate")
                {
                    Fifa.Managers.BotManager.AllocateProxies();
                }
                else
                {
                    Fifa.Managers.BotManager.DeallocateProxies();
                    Fifa.Managers.ProxyManager.ResetProxyCounter();
                }
                return "true";
            });
        }
    }
}
