using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.ActionScheduler;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Services;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;
using Newtonsoft.Json;

namespace FifaAutobuyer.WebServer.Pages
{
    public class ActionScheduler : NancyModule
    {
        public ActionScheduler()
        {
            this.RequiresAuthentication();
            Get("/actionscheduler", args => View["ActionScheduler", new ActionSchedulerModel()]);

            Post("/actionscheduler", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);
                if (parameters["action"]?.Equals("delete") == true)
                {
                    var id = int.Parse(parameters["id"]);
                    FUTSettings.Instance.ActionScheduler.RemoveAll(x => x.ID == id);
                    FUTSettings.Instance.SaveChanges();
                    Fifa.ActionScheduler.ActionScheduler.CreateScheduler();
                }
                else if (parameters["action"]?.Equals("import") == true)
                {
                    FUTSettings.Instance.ActionScheduler = new List<ActionModel>();
                    try
                    {
                        if (!String.IsNullOrEmpty(parameters["actions"]))
                        {
                            var data = JsonConvert.DeserializeObject<List<ActionModel>>(parameters["actions"]);

                            FUTSettings.Instance.ActionScheduler = data;
                            FUTSettings.Instance.SaveChanges();
                            Fifa.ActionScheduler.ActionScheduler.CreateScheduler();
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    
                }
                else
                {
                    var type = int.Parse(parameters["type"]);
                    var time = TimeSpan.Parse(parameters["time"]);
                    var percent = int.Parse(parameters["percent"]);
                    var lastId = new ActionModel(){ID = 0, Type = ActionType.None, Time = TimeSpan.Zero};
                    if (FUTSettings.Instance.ActionScheduler != null)
                    {
                        lastId = FUTSettings.Instance.ActionScheduler.LastOrDefault();
                    }
                    
                    var id = lastId?.ID + 1 ?? 0;
                    if (FUTSettings.Instance.ActionScheduler == null)
                    {
                        FUTSettings.Instance.ActionScheduler = new List<ActionModel>();
                    }

                    var model = new ActionModel() {ID = id, Type = (ActionType) type, Time = time, Description = Enum.GetName(typeof(ActionType), (ActionType)type) };
                    if (model.Type == ActionType.SetBuyPercent || model.Type == ActionType.SetSellPercent)
                    {
                        model.Percent = percent;
                        model.Description += $" ({percent})";
                    }
                    FUTSettings.Instance.ActionScheduler.Add(model);
                    FUTSettings.Instance.SaveChanges();
                    Fifa.ActionScheduler.ActionScheduler.CreateScheduler();
                }


                return Response.AsRedirect("/actionscheduler");
            });
        }
    }
}
