using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class BotManager : NancyModule
    {
        public BotManager()
        {
            this.RequiresAuthentication();
            Get("/botmanager", args =>
            {
                var model = new BotManagerModel();

                model.RunningAccounts = Fifa.Managers.BotManager.GetFutClients().Count(x => x.LogicRunningReal);
                model.TotalAccounts = Fifa.Managers.BotManager.GetFutClients().Count;

                return View["BotManager", model];
            });

            Post("/botmanager", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                if (parameters["startbot"] != null)
                {
                    if (parameters["startbot"] == "true")
                    {
                        Fifa.Managers.BotManager.StartAllBots();
                    }
                }

                if (parameters["stopbot"] != null)
                {
                    if (parameters["stopbot"] == "true")
                    {
                        Fifa.Managers.BotManager.StopAllBots();
                    }
                }

                if (parameters["startnotrunningbot"] != null)
                {
                    if (parameters["startnotrunningbot"] == "true")
                    {
                        Fifa.Managers.BotManager.StartAllNotRunningBots();
                    }
                }
                return "success";
            });

            Post("/resetpricechecks", args =>
            {
                ItemListManager.ResetPriceCheckEverywhere();
                return "";
            });
        }
    }
}
