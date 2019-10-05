using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using FifaAutobuyer.Database.Web;
using Nancy.Authentication.Basic;

namespace FifaAutobuyer.WebServer.Handler
{
    public class UserValidator : IUserValidator
    {
        public ClaimsPrincipal Validate(string username, string password)
        {
            return WebSessionsDatabase.GetWebSessions().Any(webSession => username == webSession.Username && password == webSession.Password) ? new ClaimsPrincipal(new GenericIdentity(username) ) : null;
        }
    }
}
