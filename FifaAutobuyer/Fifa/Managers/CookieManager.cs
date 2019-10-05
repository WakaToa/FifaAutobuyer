using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.Fifa.Managers
{
    public class CookieManager
    {
        private static object _lockObject = new object();
        private  static string _path => System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static CookieContainer GetCookieContainer(FUTAccount account, bool web = true)
        {
            lock (_lockObject)
            {
                var cString = web ? "web" : "mobile";
                var filename = Path.GetFullPath(_path + $"\\cookies\\{account.EMail.ToLower().Replace(".", "")}_{cString}.dat");
                if (!File.Exists(filename))
                {
                    return new CookieContainer();
                }
                Directory.CreateDirectory(Path.GetFullPath(_path + "\\cookies\\"));
                try
                {
                    using (Stream stream = File.Open(filename, FileMode.Open))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        return (CookieContainer)formatter.Deserialize(stream);
                    }
                }
                catch (Exception e)
                {
                    return new CookieContainer();
                }
            }
        }

        public static void DeleteCookieContainer(FUTAccount account, bool web = true)
        {
            lock (_lockObject)
            {
                var cString = web ? "web" : "mobile";
                var filename = Path.GetFullPath(_path + $"\\cookies\\{account.EMail.ToLower().Replace(".", "")}_{cString}.dat");
                Directory.CreateDirectory(Path.GetFullPath(_path + "\\cookies\\"));
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        public static void SaveCookieContainer(FUTAccount account, CookieContainer cookies, bool web = true)
        {
            lock (_lockObject)
            {
                var cString = web ? "web" : "mobile";
                var filename = Path.GetFullPath(_path + $"\\cookies\\{account.EMail.ToLower().Replace(".", "")}_{cString}.dat");
                Directory.CreateDirectory(Path.GetFullPath(_path + "\\cookies\\"));
                DeleteCookieContainer(account, web);
                using (Stream stream = File.Create(filename))
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, cookies);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }
    }
}
