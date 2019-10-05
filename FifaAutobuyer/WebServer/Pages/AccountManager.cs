using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class AccountManager : NancyModule
    {
        public AccountManager()
        {
            this.RequiresAuthentication();
            Get("/accountmanager", args =>
            {
                var mod = new AccountManagerModel();

                var accounts = FUTAccountsDatabase.GetFUTAccounts();
                mod.Accounts = accounts;
                return View["AccountManager", mod];
            });

            Post("/accountmanager", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var accounts = parameters["accounts"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var accString in accounts)
                {
                    if (accString.Split(';').Count() < 5)
                    {
                        continue;
                    }
                    var email = accString.Split(';')[0];
                    var password = accString.Split(';')[1];
                    var securityAnswer = accString.Split(';')[2];
                    var appAuth = accString.Split(';')[3];
                    var emailPassword = accString.Split(';')[4];
                    if (FUTAccountsDatabase.GetFUTAccountByEMail(email) == null)
                    {
                        var futAccount = new FUTAccount();
                        futAccount.EMail = email;
                        futAccount.Password = password;
                        futAccount.SecurityAnswer = securityAnswer;
                        futAccount.GoogleAuthCode = appAuth;
                        futAccount.EMailPassword = emailPassword;
                        futAccount.FUTPlatform = new FUTPlatform();
                        FUTAccountsDatabase.AddFUTAccount(futAccount);
                        Fifa.Managers.BotManager.AddBot(email);
                    }
                }
                return Response.AsRedirect("/accountmanager");
            });

            Post("/deleteaccountarray", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);

                var accounts = parameters["account"].Split(',');

                foreach (var acc in accounts)
                {
                    FUTAccountsDatabase.RemoveFUTAccountByEMail(acc);
                    Fifa.Managers.BotManager.RemoveBot(acc);
                }
                return "true";
            });

            Post("/deleteaccount", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                body = HttpUtility.UrlDecode(body);
                var parameters = HttpUtility.ParseQueryString(body);
                var acc = parameters["account"];

                FUTAccountsDatabase.RemoveFUTAccountByEMail(acc);
                Fifa.Managers.BotManager.RemoveBot(acc);
                return "true";
            });
        }
    }
}
