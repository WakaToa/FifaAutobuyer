using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Web.Performance
{
    class PerformanceCounterEx
    {
        public static int GetEthernetLoad()
        {
            return 0;
        }





        private static PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public static int GetCPULoad()
        {
            var result =  (int)_cpuCounter.NextValue();
            return result;
        }

        private static PerformanceCounter _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        private static int _ramInstalled;
        public static int GetRAMLoad()
        {

            if(_ramInstalled == 0)
            {
                ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                ManagementObjectCollection results = searcher.Get();

                foreach (ManagementObject res in results)
                {
                    _ramInstalled = int.Parse(res["TotalVisibleMemorySize"].ToString()) / 1024;
                }
            }
            

            var result = _ramInstalled - _ramCounter.NextValue();
            return (int)((result / _ramInstalled) * 100);
        }
    }
}
