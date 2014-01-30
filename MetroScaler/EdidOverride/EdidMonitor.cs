using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MetroScaler.EdidOverride
{

    class EdidMonitor
    {
        private const byte EDID_WIDTH = 21;
        private const byte EDID_HEIGHT = 22;

        public string Name;
        public bool Active;
        public string InstanceName;
        public byte[] EDID;
        public byte[] RawEDID;
        public byte Width
        {
            get { return EDID[EDID_WIDTH]; }
            set { EDID[EDID_WIDTH] = value; }
        }
        public byte Height
        {
            get { return EDID[EDID_HEIGHT]; }
            set { EDID[EDID_HEIGHT] = value; }
        }
        public double Inches
        {
            get { return Math.Sqrt(Math.Pow(this.Width,2)+Math.Pow(this.Height,2))/2.54; }
        }

        private string DeviceRegistryPath { get { return "SYSTEM\\CurrentControlSet\\Enum\\" + this.InstanceName.Substring(0, this.InstanceName.Length - 2) + "\\Device Parameters"; } }
        private string EdidOverrideRegistryPath { get { return this.DeviceRegistryPath + "\\EDID_OVERRIDE"; } }

        public void ScaleToInches(double inches)
        {
            double ratio = (double)this.Width / (double)this.Height;
            Debug.WriteLine("Ratio = " + ratio);

            double diagonal = inches * 2.54;

            double height = Math.Sqrt(Math.Pow(diagonal, 2) / (1 + Math.Pow(ratio, 2)));
            double width = ratio * height;

            this.Width = (byte)(width + 0.5);
            this.Height = (byte)(height + 0.5);

            Debug.WriteLine("New monitor width = " + this.Width + ", height=" + this.Height);
        }

        public void WriteEdidOverride()
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(this.EdidOverrideRegistryPath);

            if (registryKey != null)
            {
                this.validateEdid();
                registryKey.SetValue("0", this.EDID, RegistryValueKind.Binary);
                Debug.WriteLine("Edid override saved to registry");
            }
            registryKey.Close();
        }

        public void ResetEdidOverride()
        {
            try
            {
                Registry.LocalMachine.DeleteSubKey(this.EdidOverrideRegistryPath);
                this.Width = RawEDID[EDID_WIDTH];
                this.Height = RawEDID[EDID_HEIGHT];
            }
            catch(ArgumentException a)
            { }
            Debug.WriteLine("Edid override has been reset");
        }

        private void validateEdid()
        {
            byte checksum = 0;
            for(int i=0;i<127;i++)
            {
                checksum += this.EDID[i];
            }
            checksum = (byte)(256 - checksum);
            Debug.WriteLine("Checksum = " + checksum);
            this.EDID[127] = checksum;
        }

        override public string ToString()
        {
            return this.Name;
        }
    }
}
