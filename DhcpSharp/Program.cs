using PcapDotNet.Core;
using System;
using System.Net;

namespace DhcpSharp
{
    class Program
    {
        //--Classes
        private static Localhost localhost = new Localhost();
        private static AddressPool addresspool = new AddressPool();
        private static Interface inter = new Interface(localhost);
        private static Service service = new Service(localhost, addresspool, inter);

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
                LivePacketDevice device = localhost.getUseableInterfaces()[i];
                if (device.Description != null)
                {
                    Console.WriteLine(i + ": " + device.Description);
                }
                else
                {
                    Console.WriteLine(i + ": " + " (No description available)");
                }

                // Print IP-Information for each Interface
                printInterfaceInfo(localhost.getUseableInterfaces()[i]);
            }

            Console.WriteLine();

            //--Set the interface by Interface-Index
            Console.Write("Enter the interface number to select: ");
            int interfaceIndex = Convert.ToInt32(Console.ReadLine());

            inter.setInterfaceIndex(interfaceIndex);
            localhost.setActiveInterface(interfaceIndex);

            Console.Clear();

            //--Define the Gateway
            Console.Write("Please set the Gateway-IPv4: ");
            addresspool.setGatewayIpAddress(IPAddress.Parse(Console.ReadLine()));

            //--Define Domain Name
            Console.Write("Please enter Domain Name: ");
            addresspool.setDomainName(Console.ReadLine());

            //--Define the Addresspool
            Console.Write("Please set start-IPv4: ");
            string start = Console.ReadLine();
            Console.Write("Please set end-IPv4: ");
            string end = Console.ReadLine();

            addresspool.setAddressScope(IPAddress.Parse(start), IPAddress.Parse(end));
        }

        private static void printInterfaceInfo(IPacketDevice pDevice)
        {
            foreach (DeviceAddress address in pDevice.Addresses)
            {
                if (address.Address != null & address.Address.Family.Equals(SocketAddressFamily.Internet))
                {
                    Console.WriteLine("\tAddress: " + address.Address);
                }
            }
        }
    }
}
