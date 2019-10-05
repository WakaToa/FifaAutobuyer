using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Managers
{
    public class AppSettingsManager
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["FUTDatabase"].ConnectionString;
        }

        public static int GetWebappPort()
        {
            return int.Parse(ConfigurationManager.AppSettings["WebAppPort"]);
        }

        public static string GetInstanceDescription()
        {
            return ConfigurationManager.AppSettings["InstanceDescription"];
        }

        public static string GetAntiCaptchaKey()
        {
            return ConfigurationManager.AppSettings["AntiCaptchaKey"];
        }

        public static List<string> GetAntiCaptchaProxys()
        {
            return ConfigurationManager.AppSettings["AntiCaptchaProxies"].Split(',').ToList();
        }
    }
}
