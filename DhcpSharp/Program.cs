using Newtonsoft.Json;
using SharpPcap;
using System;
using System.Diagnostics;
using System.IO;

namespace DhcpSharp
{
    class Program
    {
        //--Classes
        private static Localhost localhost = new Localhost();
        private static SubnetList subnetList = new SubnetList();
        private static Service service = new Service(localhost, subnetList);


        static void Main(string[] args)
        {
            //--Get and set the local interface to use
            initializeLocalhost();

            //--Clear Console with basic configuration
            Console.Clear();

            //--Wait for Dhcp Discovery messages
            service.startListen();

            Console.Read();
        }

        private static void initializeLocalhost()
        {
            if (localhost.getUseableInterfaces().Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return;
            }

            // Print the list
            for (int i = 0; i != localhost.getUseableInterfaces().Count; ++i)
            {
                ILiveDevice device = localhost.getUseableInterfaces()[i];
                if (device.Description != null)
                {
                    Console.WriteLine(i + ": " + device.Description);
                }
                else
                {
                    Console.WriteLine(i + ": " + " (No description available)");
                }
            }

            Console.WriteLine();

            //--Set the interface by Interface-Index
            Console.Write("Enter the interface number to select: ");
            int interfaceIndex = Convert.ToInt32(Console.ReadLine());

            localhost.setActiveInterface(interfaceIndex);

            Console.Clear();

            //--Read subnets from Config-file
            using (StreamReader streamReader = new StreamReader("config"))
            {
                while (!streamReader.EndOfStream)
                {
                    Subnet tempSubnet = JsonConvert.DeserializeObject<Subnet>(streamReader.ReadLine());

                    if (tempSubnet != null)
                    {
                        tempSubnet.calculateAddresses();
                        subnetList.list.Add(tempSubnet);
                    }
                }
            }

            foreach (Subnet subnet in subnetList.list)
            {
                Debug.WriteLine("listenIp:{0} dhcpIp:{1} dnsIp:{2} gatewayIp:{3} Domain Name:{4} Range-Start:{5} Range-End:{6} Netmask:{7}", subnet.listenIp, subnet.dhcpIp, subnet.dnsIp, subnet.gatewayIp, subnet.domainName, subnet.rangeStartIp, subnet.rangeEndIp, subnet.netmask);

            }

            Debug.WriteLine("Subnet configurations found: {0}", subnetList.list.Count);
        }
    }
}
