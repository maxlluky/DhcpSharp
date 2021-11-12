using Newtonsoft.Json;
using PcapDotNet.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DhcpSharp
{
    class Program
    {
        //--Classes
        private static Interface localhost = new Interface();
        private static Config subnetList = new Config();
        private static DhcpService service = new DhcpService(localhost, subnetList);

        static void Main(string[] args)
        {
            //--Get and set the local interface to use
            initializeService();

            //--Clear Console with basic configuration
            Console.Clear();

            //--Wait for Dhcp Discovery messages
            service.startListen();

            Console.Read();
        }

        private static void initializeService()
        {
            if (localhost.getUseableInterfaces().Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return;
            }

            for (int i = 0; i != localhost.getUseableInterfaces().Count; ++i)
            {
                PacketDevice device = localhost.getUseableInterfaces()[i];
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
            if (!File.Exists("config"))
            {
                Debug.WriteLine("Error! Canot find the config-file. Please create config-file!");
                Console.WriteLine("Error! Canot find the config-file. Please create config-file!");
                Environment.Exit(0);
            }

            using (StreamReader streamReader = new StreamReader("config"))
            {
                while (!streamReader.EndOfStream)
                {
                    Subnet tempSubnet = JsonConvert.DeserializeObject<Subnet>(streamReader.ReadLine());

                    if (tempSubnet != null)
                    {
                        tempSubnet.calculateAddresses();
                        subnetList.subnetList.Add(tempSubnet);
                    }
                }
            }

            Console.Title = "DhcpSharp v." + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
