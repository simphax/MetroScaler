using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;
using Microsoft.Win32;

namespace MetroScaler.EdidOverride
{
    class EdidOverrideUtils
    {

        public static void test()
        {
            Debug.WriteLine("test");

            List<EdidMonitor> monlist = GetMonitorList();

            monlist[0].ScaleToInches(12);
            monlist[0].WriteEdidOverride();

            Debug.WriteLine("done");
        }

        public static List<EdidMonitor> GetMonitorList()
        {
            List<EdidMonitor> monitorList = new List<EdidMonitor>();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorID");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    EdidMonitor monitorInfo = new EdidMonitor();

                    Debug.WriteLine("Active: {0}", queryObj["Active"]);
                    monitorInfo.Active = (bool)queryObj["Active"];

                    Debug.WriteLine("InstanceName: {0}", queryObj["InstanceName"]);
                    monitorInfo.InstanceName = (string)queryObj["InstanceName"];

                    StringBuilder sbuilder = new StringBuilder();
                    for (int i = 0; i < (UInt16)queryObj["UserFriendlyNameLength"];i++ )
                    {
                        sbuilder.Append((char)((UInt16[])queryObj["UserFriendlyName"])[i]);
                    }

                    Debug.WriteLine("UserFriendlyName: " + sbuilder.ToString());
                    monitorInfo.Name = sbuilder.ToString();

                    string PNP = queryObj["InstanceName"].ToString();
                    PNP = PNP.Substring(0, PNP.Length - 2);  // remove _0
                    if (PNP != null && PNP.Length > 0)
                    {
                        string registryPath = "SYSTEM\\CurrentControlSet\\Enum\\" + PNP + "\\" + "Device Parameters\\";
                        Debug.WriteLine("Registry path: " + registryPath);

                        RegistryKey regKey = Registry.LocalMachine.OpenSubKey(registryPath, false);
                        if (regKey != null)
                        {
                            if (regKey.GetValueKind("edid") == RegistryValueKind.Binary)
                            {
                                Debug.WriteLine("read edid");

                                byte[] edid = (byte[])regKey.GetValue("edid");

                                byte sum = 0;
                                foreach(byte b in edid)
                                {
                                    sum += b;
                                }

                                Debug.WriteLine("EDID length: {0}", edid.Length);
                                Debug.WriteLine("EDID sum: {0}", sum);

                                if(edid.Length == 128 && sum == 0) //We have a valid EDID
                                {
                                    monitorInfo.EDID = edid;

                                    Debug.WriteLine("Monitor width = " + monitorInfo.Width + ", height=" + monitorInfo.Height);

                                    monitorList.Add(monitorInfo);
                                }

                            }
                            regKey.Close();
                        }
                    }

                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
            }

            return monitorList;
        }

    }
}
