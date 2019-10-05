using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses;
using Newtonsoft.Json;

namespace FifaAutobuyer.WebServer.Handler
{
    public class AuthenticationFailedHandler : IStatusCodeHandler
    {
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.Forbidden;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.Response = new RedirectResponse("/?forbidden=true");
        }
    }
}
