using DhcpDotNet;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;

class Service
{
    //--Classes
    private static Localhost localhost;
    private readonly Interface inter;
    private AddressPool addressPool;
    private Builder builder;

    //--Active Device
    private PacketDevice packetDevice;
    private PacketCommunicator packetCommunicator;

    public Service(Localhost pLocalhost, AddressPool pAddressPool, Interface pInter)
    {
        localhost = pLocalhost;
        addressPool = pAddressPool;
        inter = pInter;
    }

    public void startListen()
    {
        //--Get active Interface/Device to use
        packetDevice = localhost.getActiveInterface();

        // Open the device
        packetCommunicator = packetDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);

        Console.WriteLine("Listening on " + packetDevice.Description + "...");
        Console.WriteLine("Status\t\t\tDestination MAC\t\tDHCP Message\tTransaction ID\t\tServer Identifier");
        Console.WriteLine("===========================================================================================================");

        //--Create Builderclass to build the networkpackets
        builder = new Builder(addressPool, inter);

        //--Set a filter to reduce the traffic
        packetCommunicator.SetFilter("udp");

        // start the capture
        packetCommunicator.ReceivePackets(0, receiveCallback);

    }

    // Callback function invoked by Pcap.Net for every incoming packet
    private void receiveCallback(Packet packet)
    {
        try
        {
            //--Parsing the layer above Ethernet
            IpV4Datagram ipPacket = packet.Ethernet.IpV4;
            UdpDatagram udpDatagram = ipPacket.Udp;

            Datagram datagram = udpDatagram.Payload;

            //--Check if Packet is DHCP with Ports.
            if (udpDatagram.SourcePort.Equals(68) & udpDatagram.DestinationPort.Equals(67))
            {
                //--Create a new DhcpPacket to parse the received and read Data from it.
                DHCPv4Packet dhcpv4Packet = new DHCPv4Packet();

                if (dhcpv4Packet.parsePacket(datagram.ToArray()))
                {
                    //--Create a dhcpOption.             
                    List<DHCPv4Option> list = new DHCPv4Option().parseDhcpOptions(dhcpv4Packet.dhcpOptions);

                    foreach (DHCPv4Option dhcpMessageTypeOption in list)
                    {
                        if (dhcpMessageTypeOption.optionIdBytes.Equals(0x35))
                        {
                            switch (dhcpMessageTypeOption.optionValue[0])
                            {
                                case 0x01:
                                    Console.WriteLine("Service received:\t" + packet.Ethernet.Destination + "\tDISCOVER\txid: " + BitConverter.ToString(dhcpv4Packet.xid));

                                    //--Sending an Dhcp Offer                             
                                    sendDhcpOffer(new MacAddress(inter.getHwAddress()), packet.Ethernet.Source, dhcpv4Packet.xid, dhcpv4Packet.secs);

                                    break;
                                case 0x03:
                                    foreach (DHCPv4Option dhcpServerIdentifierOption in list)
                                    {
                                        if (dhcpServerIdentifierOption.optionIdBytes.Equals(0x36))
                                        {
                                            if (BitConverter.ToInt32(dhcpServerIdentifierOption.optionValue, 0).Equals(BitConverter.ToInt32(inter.getIPAddress().GetAddressBytes(), 0)))
                                            {
                                                Console.WriteLine("Service received:\t" + packet.Ethernet.Destination + "\tREQUEST\t\txid: " + BitConverter.ToString(dhcpv4Packet.xid) + "\tSID: " + BitConverter.ToString(dhcpServerIdentifierOption.optionValue, 0));

                                                //--Sending Dhcp Ack  
                                                sendDhcpAck(new MacAddress(inter.getHwAddress()), packet.Ethernet.Source, dhcpv4Packet.xid, dhcpv4Packet.secs);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client preferes other DHCP-Server!");
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The DHCP-Message could not be parsed...");
                }
            }
        }
        catch (Exception) { }
    }

    public void sendDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tOFFER\t\txid: " + BitConverter.ToString(pTransactionId));

        //--Send down the packet. First build the packet.
        packetCommunicator.SendPacket(builder.buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
    }

    public void sendDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tACK\t\txid: " + BitConverter.ToString(pTransactionId));

        //--Send down the packet. First build the packet. The Method "buildDhcpAck" will decide if it sends a ACK or NAK.
        packetCommunicator.SendPacket(builder.buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
    }
}
