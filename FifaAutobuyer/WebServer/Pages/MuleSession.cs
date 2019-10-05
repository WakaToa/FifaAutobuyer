using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using System.Web;
using FifaAutobuyer.WebServer.Models;

namespace FifaAutobuyer.WebServer.Pages
{
    public class MuleSession : NancyModule
    {
        public MuleSession()
        {
            Get("/mulesession", args =>
            {
                var model = new MuleSessionModel();

                var parameters = HttpUtility.ParseQueryString(Request.Url.Query);
                var email = parameters["email"];
                var muleAccount = Fifa.Managers.MuleManager.GetMuleClientByEMail(email);
                model.MuleClient = muleAccount;
                if (muleAccount != null)
                { 
                    return View["MuleSession", model];
                }
                return Response.AsRedirect("/mulemanager");
            });

            Post("/getmulestatus", args =>
            {
                var responseText = "";

                var parameters = HttpUtility.ParseQueryString(Request.Url.Query);
                var email = parameters["email"];

                var muleAccount = Fifa.Managers.MuleManager.GetMuleClientByEMail(email);

                if (muleAccount == null) return Response.AsText(responseText);
                if (muleAccount.GetMulingStatus() == null) return Response.AsText(responseText);

                var mulestatus = muleAccount.GetMulingStatus();
                var mulestatussorted = mulestatus.OrderBy(o => o.DateTime).ToList();
                mulestatussorted.Reverse();
                foreach (var status in mulestatussorted)
                {
                    responseText += "<tr>";
                    responseText += "<td>" + status.EMail + "</td>";
                    responseText += $"<td>{status.DateTime:d/M/yyyy HH:mm:ss}</td>";
                    responseText += "<td>" + status.Message + "</td>";
                    responseText += "</tr>";
                }

                return Response.AsText(responseText);
            });

            Post("/startmulesession", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);
                var email = parameters["email"];
                var muleAccount = Fifa.Managers.MuleManager.GetMuleClientByEMail(email);
                if (muleAccount == null)
                {
                    return Response.AsRedirect("/mulemanager");
                }
                muleAccount.StartMule();
                return Response.AsRedirect("/mulesession?email=" + email);
            });

            Post("/stopmulesession", args =>
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var parameters = HttpUtility.ParseQueryString(body);
                var email = parameters["email"];
                var muleAccount = Fifa.Managers.MuleManager.GetMuleClientByEMail(email);
                if (muleAccount == null)
                {
                    return Response.AsRedirect("/mulemanager");
                }
                muleAccount.StopMule();
                return Response.AsRedirect("/mulesession?email=" + email);
            });
        }
    }
}
