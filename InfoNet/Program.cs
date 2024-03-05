using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;

namespace InfoNet
{
    class Program
    {
        static void Main(string[] args)
        {
            int pocetSpojeni = ZjistiAktivniSpojeni();
            while (pocetSpojeni == 0)
            {
                Console.WriteLine("Asi není připojen síťový kabel. Zkouším znovu...");
                pocetSpojeni = ZjistiAktivniSpojeni();
                System.Threading.Thread.Sleep(2000);
            }

            var objSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");

            List<string> pole = new List<string>();
            string aktivniSpojeni = $"Počet aktivních spojení: {pocetSpojeni}";
            string defaultGateway = $"\nDefault Gateway: {GetDefaultGatewayUsingIpConfig()}";

            pole.Add($"{aktivniSpojeni}{defaultGateway}");

            foreach (ManagementObject obj in objSearcher.Get())
            {
                if (obj["IPAddress"] == null || obj["IPSubnet"] == null) continue;

                string ipAdresa = ((string[])obj["IPAddress"])[0];
                string maska = ((string[])obj["IPSubnet"])[0];
                bool dhcp = (bool)obj["DHCPEnabled"];
                string oznameniIP = dhcp ? "DHCP              povoleno" : "DHCP              zakázán";
                oznameniIP += $"\nIP adresa         {ipAdresa}";
                oznameniIP += $"\nMaska             {maska}";

                string mac = (string)obj["MACAddress"];
                if (mac != null)
                {
                    oznameniIP += $"\nMAC adresa        {mac}";
                }
                else
                {
                    oznameniIP += $"\nMAC adresa        nenalezeno";
                }

                pole.Add($"{oznameniIP}");
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pole.Count; i++)
            {
                sb.AppendLine(pole[i]);
                if (i < pole.Count - 1)
                {
                    sb.AppendLine("--------");
                }
            }

            Console.WriteLine(sb.ToString());

            Console.ReadKey();
        }

        static int ZjistiAktivniSpojeni()
        {
            var objSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus=2");
            var objCollection = objSearcher.Get();
            return objCollection.Count;
        }

        public static string GetDefaultGatewayUsingIpConfig()
        {
            ProcessStartInfo psi = new ProcessStartInfo("ipconfig", "/all");
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            string output;
            using (Process process = Process.Start(psi))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            string gateway = null;
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Default Gateway"))
                {
                    gateway = line.Split(':').Last().Trim();
                    if (!string.IsNullOrEmpty(gateway) && gateway != "0.0.0.0")
                    {
                        break;
                    }
                }
            }

            return gateway;
        }
    }
}
