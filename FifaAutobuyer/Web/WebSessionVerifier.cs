using FifaAutobuyer.Database.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Web
{
    public class WebSessionVerifier
    {
        public static bool VerifySessionID(string sessionID, string salt)
        {
            if(string.IsNullOrEmpty(sessionID) || string.IsNullOrWhiteSpace(sessionID))
            {
                return false;
            }

            var sessions = WebSessionsDatabase.GetWebSessions();

            var currentSession = sessions.Where(x => x.SessionID == sessionID && x.Salt == salt).FirstOrDefault();

            if(currentSession == null)
            {
                return false;
            }
            if(!string.IsNullOrEmpty(currentSession.Username) && currentSession.SessionID == sessionID && currentSession.Salt == salt)
            {
                return true;
            }
            return false;
        }

        public static bool CheckUsernamePassword(string username, string password)
        {
            var sessions = WebSessionsDatabase.GetWebSessions();
            var currentSession = sessions.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefault();

            if (currentSession == null)
            {
                return false;
            }
            if (currentSession.Password == password)
            {
                return true;
            }
            return false;
        }
    }
}
