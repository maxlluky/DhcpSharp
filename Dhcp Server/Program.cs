using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dhcp_Server
{
    class Program
    {
        private static Localhost localhost = new Localhost();
        private static AddressPool addresspool = new AddressPool();

        static void Main(string[] args)
        {
            //--Get and set the local interface to use
            initializeLocalhost();

            addresspool.setAddressScope(IPAddress.Parse("172.16.0.100"), IPAddress.Parse("172.16.0.200"));

            //--Wait for Dhcp Discovery messages
            listen();

            Console.Read();
        }

        private static void listen()
        {
            //--Get active Interface/Device to use
            PacketDevice packetDevice = localhost.getActiveInterface();
            // Open the device
            using (PacketCommunicator communicator = packetDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))                                 
            {
                Console.WriteLine("Listening on " + packetDevice.Description + "...");

                //--Set a filter to reduce the traffic
                communicator.SetFilter("udp");

                // start the capture
                communicator.ReceivePackets(0, receiveCallback);
            }
        }

        // Callback function invoked by Pcap.Net for every incoming packet
        private static void receiveCallback(Packet packet)
        {
            //--Parsing the layer above Ethernet
            IpV4Datagram ipPacket = packet.Ethernet.IpV4;
            UdpDatagram udpDatagram = ipPacket.Udp;

            //--Writes out Packet-Information
            Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length + "\t" + ipPacket.Source + ":" + udpDatagram.SourcePort + "\t" + ipPacket.Destination + ":" + udpDatagram.DestinationPort);

            //--Check if Packet is DHCP-Discover (= 300 Bytes)
            if (ipPacket.Source.ToString() == "0.0.0.0" & ipPacket.Destination.ToString() == "255.255.255.255" & udpDatagram.DestinationPort == (ushort)67 & udpDatagram.Payload.Length == 300)
            {
                //--Temp. send offer to client
                sendDhcpOffer(packet.Ethernet.Source);
            }
        }

        private static void sendDhcpOffer(MacAddress pSourceMacAddress)
        {

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

            // Set the interface 
            Console.Write("Enter the interface number to select: ");
            localhost.setActiveInterface(Convert.ToInt32(Console.ReadLine()));

            //--Define Addresspool

        }

        private static void printInterfaceInfo(IPacketDevice pDevice)
        {
            foreach (DeviceAddress address in pDevice.Addresses)
            {
                Console.WriteLine("\tAddress Family: " + address.Address.Family);

                if (address.Address != null)
                    Console.WriteLine(("\tAddress: " + address.Address));
                if (address.Netmask != null)
                    Console.WriteLine(("\tNetmask: " + address.Netmask));
                if (address.Broadcast != null)
                    Console.WriteLine(("\tBroadcast Address: " + address.Broadcast));
                if (address.Destination != null)
                    Console.WriteLine(("\tDestination Address: " + address.Destination));
            }
            Console.WriteLine();
        }
    }
}
