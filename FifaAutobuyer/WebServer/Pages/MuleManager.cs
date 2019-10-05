using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using FifaAutobuyer.Database.Web;
using FifaAutobuyer.Fifa;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.MuleApi;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class MuleManager : NancyModule
    {
        private WebAccessRole GetRoleFromClaim(string claim)
        {
            if (string.IsNullOrEmpty(claim))
            {
                return WebAccessRole.None;
            }
            var role = WebSessionsDatabase.GetWebSessions().FirstOrDefault(x => x.Username.ToLower() == claim.ToLower());
            if (role == null)
            {
                return WebAccessRole.None;
            }
            return role.Role;
        }
        public MuleManager()
        {
            this.RequiresAuthentication();
            this.RequiresClaims(delegate(Claim claim)
            {
                var beingAs = GetRoleFromClaim(claim.Value);

                return beingAs == WebAccessRole.Administrator || beingAs == WebAccessRole.Owner;
            });
            Get("/mulemanager", args =>
            {
                var model = new MuleManagerModel();


                model.Clients = Fifa.Managers.MuleManager.GetMuleClients();


                return View["MuleManager", model];
            });

            Post("/mulemanager", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);
                if (parameters["importMulingsessions"] != null)
                {
                    try
                    {
                        var data = parameters["importData"];

                        var lines = data.Split("\r\n".ToCharArray());

                        foreach (var line in lines)
                        {
                            if (line.Trim() == "" || !line.Contains(";"))
                            {
                                continue;
                            }
                            var accountData = line.Split(';');
                            if (accountData.Count() != 8)
                            {
                                continue;
                            }
                            var futAccount = new FUTAccount
                            {
                                EMail = accountData[0],
                                Password = accountData[1],
                                EMailPassword = accountData[2],
                                SecurityAnswer = accountData[3],
                                BackupCode1 = accountData[4],
                            };
                            var muleClient = new MuleClient(futAccount)
                            {
                                MuleVolume = int.Parse(accountData[5]),
                                MinimumCoinsOnAccount = int.Parse(accountData[6])
                            };
                            Fifa.Managers.MuleManager.AddMuleClient(muleClient);
                        }
                        return Response.AsRedirect("/mulemanager");
                    }
                    catch (Exception e)
                    {
                        return Response.AsRedirect("/mulemanager");
                    }
                }
                if (parameters["addMulingsession"] != null)
                {
                    var futAccount = new FUTAccount
                    {
                        EMail = parameters["accountEMail"],
                        EMailPassword = parameters["accountEMailPassword"],
                        Password = parameters["accountPassword"],
                        BackupCode1 = parameters["accountBackupCode"],
                        SecurityAnswer = parameters["accountSecurityanswer"]
                    };
                    var muleClient = new MuleClient(futAccount)
                    {
                        MuleVolume = int.Parse(parameters["muleVolume"]),
                        MinimumCoinsOnAccount = int.Parse(parameters["minimumCoinsOnAccount"])
                    };
                    Fifa.Managers.MuleManager.AddMuleClient(muleClient);

                    return Response.AsRedirect("/mulemanager");
                }
                return Response.AsRedirect("/mulemanager");
            });

            Post("/removemulesession", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);

                var email = parameters["email"];

                var client = Fifa.Managers.MuleManager.GetMuleClientByEMail(email);
                if (client == null) return Response.AsRedirect("/mulemanager");
                if (!client.Muling)
                {
                    Fifa.Managers.MuleManager.RemoveMuleClient(email);
                }
                return Response.AsRedirect("/mulemanager");
            });
        }
    }
}
