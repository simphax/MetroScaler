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

        private static string convertUIntToString(UInt16[] array)
        {
            int length = 1;
            while (length < array.Length && array[length] != 0)
            {
                length++;
            }
            char[] charArray = new char[length];
            Array.Copy(array, charArray, length);
            return new String(charArray);
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

                    string name = convertUIntToString((UInt16[])queryObj["UserFriendlyName"]);
                    
                    Debug.WriteLine("UserFriendlyName: " + name);
                    monitorInfo.Name = name;

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

                                    string instanceName = (string)queryObj["InstanceName"];
                                    instanceName = instanceName.Replace("\\","\\\\");
                                    string query = String.Format("SELECT * FROM WmiMonitorDescriptorMethods WHERE InstanceName = '{0}'",instanceName);
                                    //string query = "SELECT * FROM WmiMonitorDescriptorMethods";
                                    ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("root\\WMI", query);
                                    Debug.WriteLine("-----------------");
                                    foreach (ManagementObject descMethodObj in searcher2.Get())
                                    {
                                        ManagementBaseObject inParams = descMethodObj.GetMethodParameters("WmiGetMonitorRawEEdidV1Block");
                                        inParams["BlockId"] = 0;

                                        ManagementBaseObject outParams = null;
                                        outParams = descMethodObj.InvokeMethod("WmiGetMonitorRawEEdidV1Block", inParams, null);
                                        Debug.WriteLine("Returned a block of type {0}, having a content of type {1} ",
                                                            outParams["BlockType"], outParams["BlockContent"].GetType());
                                        Debug.WriteLine("{0}", ((byte[])outParams["BlockContent"]).Length);

                                        byte[] rawedid = ((byte[])outParams["BlockContent"]);

                                        monitorInfo.RawEDID = rawedid;
                                    }

                                    monitorList.Add(monitorInfo);
                                }

                            }
                        }
                        regKey.Close();
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
