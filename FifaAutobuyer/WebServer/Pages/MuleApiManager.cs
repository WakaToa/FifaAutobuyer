using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Database.Web;
using FifaAutobuyer.Fifa.MuleApi.Clients;
using FifaAutobuyer.WebServer.Models;
using Nancy;
using Nancy.Security;

namespace FifaAutobuyer.WebServer.Pages
{
    public class MuleApiManager : NancyModule
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

        public MuleApiManager()
        {
            this.RequiresAuthentication();
            this.RequiresClaims(delegate(Claim claim)
            {
                var beingAs = GetRoleFromClaim(claim.Value);

                return beingAs == WebAccessRole.Administrator || beingAs == WebAccessRole.Owner;
            });
            Get("/muleapimanager", args =>
            {
                var model = new MuleApiManagerModel();

                return View["MuleApiManager", model];
            });

            Post("/muleapimanager", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);
                if (parameters["action"] == "startstop")
                {
                    switch (parameters["id"])
                    {
                        case "gte":
                            if (GameTradeEasyApiClient.Client.Running)
                            {
                                GameTradeEasyApiClient.Client.StopLogicRoutine();
                            }
                            else
                            {
                                GameTradeEasyApiClient.Client.StartLogicRoutine();
                            }
                            break;
                        case "mf":
                            if (MuleFactoryApiClient.Client.Running)
                            {
                                MuleFactoryApiClient.Client.StopLogicRoutine();
                            }
                            else
                            {
                                MuleFactoryApiClient.Client.StartLogicRoutine();
                            }
                            break;
                        case "ws":
                            if (WholeSaleApiClient.Client.Running)
                            {
                                WholeSaleApiClient.Client.StopLogicRoutine();
                            }
                            else
                            {
                                WholeSaleApiClient.Client.StartLogicRoutine();
                            }
                            break;
                    }
                }
                else
                {
                    FUTSettings.Instance.GTEPartnerID = parameters["GTEPartnerID"];
                    FUTSettings.Instance.GTEPartnerApiKey = parameters["GTEPartnerApiKey"];
                    if (FUTSettings.Instance.GTEClientPlatform != parameters["GTEClientPlatform"])
                    {
                        FUTSettings.Instance.GTEClientPlatform = parameters["GTEClientPlatform"];
                        GameTradeEasyApiClient.Client = new GameTradeEasyApiClient(FUTSettings.Instance.GTEClientPlatform);
                    }
                    FUTSettings.Instance.MuleFactoryUser = parameters["MuleFactoryUser"];
                    FUTSettings.Instance.MuleFactorySecretWord = parameters["MuleFactorySecretWord"];
                    if (FUTSettings.Instance.MuleFactoryClientPlatform != parameters["MuleFactoryClientPlatform"])
                    {
                        FUTSettings.Instance.MuleFactoryClientPlatform = parameters["MuleFactoryClientPlatform"];
                        MuleFactoryApiClient.Client = new MuleFactoryApiClient(FUTSettings.Instance.MuleFactoryClientPlatform);
                    }
                    FUTSettings.Instance.WholeSaleApiKey = parameters["WholeSaleApiKey"];
                    if (FUTSettings.Instance.WholeSaleClientPlatform != parameters["WholeSaleClientPlatform"])
                    {
                        FUTSettings.Instance.WholeSaleClientPlatform = parameters["WholeSaleClientPlatform"];
                        WholeSaleApiClient.Client = new WholeSaleApiClient(FUTSettings.Instance.WholeSaleClientPlatform);
                    }
                    FUTSettings.Instance.MuleApiMaxSellPerDayPerAccount = int.Parse(parameters["MuleApiMaxSellPerDayPerAccount"]);
                    FUTSettings.Instance.MuleApiMaxTransactionValue = int.Parse(parameters["MuleApiMaxTransactionValue"]);
                    FUTSettings.Instance.MuleApiMinCoinsOnAccount = int.Parse(parameters["MuleApiMinCoinsOnAccount"]);
                    FUTSettings.Instance.MuleApiRequestDelay = int.Parse(parameters["MuleApiRequestDelay"]);
                    FUTSettings.Instance.MuleApiSellDelayPerAccount = int.Parse(parameters["MuleApiSellDelayPerAccount"]);

                    FUTSettings.Instance.SaveChanges();
                }

                return Response.AsRedirect("/muleapimanager");
            });
        }
    }
}
