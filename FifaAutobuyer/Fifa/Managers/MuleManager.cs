using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using FifaAutobuyer.Fifa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.MuleApi;

namespace FifaAutobuyer.Fifa.Managers
{
    public class MuleManager
    {
        private static List<MuleClient> _muleClients = new List<MuleClient>();
        private static object _muleLock = new object();
        public static void AddMuleClient(MuleClient client)
        {
            lock (_muleLock)
            {
                _muleClients.Add(client);
            }
        }
        public static void RemoveMuleClient(string email)
        {
            lock (_muleLock)
            {
                var client = _muleClients.Where(x => x.DestinationFUTAccount.EMail.ToLower() == email).FirstOrDefault();
                if(client != null)
                {
                    _muleClients.Remove(client);
                }
            }
        }
        public static MuleClient GetMuleClientByEMail(string email)
        {
            lock (_muleLock)
            {
                return _muleClients.Where(x => x.DestinationFUTAccount.EMail == email).FirstOrDefault();
            }
        }
        public static List<MuleClient> GetMuleClients()
        {
            lock (_muleLock)
            {
                return _muleClients;
            }
        }
    }
}
