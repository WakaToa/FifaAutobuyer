using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class NotificationCenter : NancyModule
    {
        public NotificationCenter()
        {
            this.RequiresAuthentication();
            Get("/notificationcenter", args =>
            {
                var mod = new NotificationCenterModel();

                var notifications = FUTLogsDatabase.GetFUTNotifications();
                notifications.Reverse();

                mod.Notifications = new List<Tuple<FUTNotification, string>>();

                foreach (var futNotification in notifications)
                {
                    mod.Notifications.Add(new Tuple<FUTNotification, string>(futNotification, $"{Fifa.Services.Helper.TimestampToDateTime(futNotification.Timestamp):d/M/yyyy HH:mm:ss}"));
                }
                return View["NotificationCenter", mod];
            });

            Post("/acknowledgenotificationarray", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var notifications = parameters["notification"].Split(',');

                foreach (var not in notifications)
                {
                    var id = int.Parse(not);
                    FUTLogsDatabase.RemoveFUTNotification(id);
                }
                return "true";
            });

            Post("/acknowledgenotification", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);
                var notification = int.Parse(parameters["notification"]);

                FUTLogsDatabase.RemoveFUTNotification(notification);
                return "true";
            });
        }
    }
}
