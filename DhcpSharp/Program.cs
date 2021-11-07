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
        private static Interface inter = new Interface(localhost);
        private static Service service = new Service(localhost, inter, subnetList);


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

            inter.setInterfaceIndex(interfaceIndex);
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
                Debug.WriteLine("VLAN: {0} Domain Name: {1} Gateway {2} Range-Start {3} Range-End {4}", subnet.vlanID, subnet.domainName, subnet.gatewayIp, subnet.rangeStartIp, subnet.rangeEndIp);

            }

            Debug.WriteLine("Subnet configurations found: {0}", subnetList.list.Count);
        }
    }
}
