using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class ExportData : NancyModule
    {
        public ExportData()
        {
            this.RequiresAuthentication();

            Get("/export/accounts", args =>
            {
                var exportText = "";
                var accs = FUTAccountsDatabase.GetFUTAccounts();

                foreach (var futAccount in accs)
                {
                    exportText +=
                        $"{futAccount.EMail};{futAccount.Password};{futAccount.SecurityAnswer};{futAccount.GoogleAuthCode};{futAccount.EMailPassword}{Environment.NewLine}";
                }

                return Response.AsText(exportText);
            });

            Get("/export/proxies", args =>
            {
                using (var ctx = new FUTSettingsDatabase())
                {
                    var proxys = ctx.FUTProxys.ToList();
                    var exportText = "";
                    foreach (var proxy in proxys)
                    {
                        exportText +=
                            $"{proxy.Host}:{proxy.Port}:{proxy.Username}:{proxy.Password}{Environment.NewLine}";
                    }

                    return Response.AsText(exportText);
                }
                
            });

            Get("/export/actionscheduler", args =>
            {
                return Response.AsText(FUTSettings.Instance.ActionSchedulerJson);
            });
        }
    }
}
