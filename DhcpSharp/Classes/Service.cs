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

    public Service(Localhost pLocalhost, AddressPool pAddressPool, Interface pInter)
    {
        localhost = pLocalhost;
        addressPool = pAddressPool;
        inter = pInter;
    }

    public void startListen()
    {
        //--Get active Interface/Device to use
        PacketDevice packetDevice = localhost.getActiveInterface();
        // Open the device
        using (PacketCommunicator communicator = packetDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
        {
            Console.WriteLine("Listening on " + packetDevice.Description + "...");
            Console.WriteLine("Status\t\t\tDestination MAC\t\tDHCP Message\tTransaction ID");
            Console.WriteLine("===================================================================================");

            //--Create Builderclass to build the networkpackets
            builder = new Builder(addressPool, inter);

            //--Set a filter to reduce the traffic
            communicator.SetFilter("udp");

            // start the capture
            communicator.ReceivePackets(0, receiveCallback);
        }
    }

    // Callback function invoked by Pcap.Net for every incoming packet
    private void receiveCallback(Packet packet)
    {
        //--Parsing the layer above Ethernet
        IpV4Datagram ipPacket = packet.Ethernet.IpV4;
        UdpDatagram udpDatagram = ipPacket.Udp;

        Datagram datagram = udpDatagram.Payload;

        //--Check if Packet is DHCP with Ports.
        if (udpDatagram.SourcePort.Equals(68) & udpDatagram.DestinationPort.Equals(67))
        {
            //--Create a new DhcpPacket to parse the received and read Data from it.
            DhcpPacket receivedDhcp = new DhcpPacket();
            receivedDhcp.parsePacket(datagram.ToArray());

            //--Create a dhcpOption.             
            List<DhcpOption> list = new DhcpOption().parseDhcpOptions(receivedDhcp.dhcpOptions);

            foreach (DhcpOption item in list)
            {
                if (item.optionIdBytes.Equals(0x35))
                {
                    switch (item.optionValue[0])
                    {
                        case 0x01:
                            Console.WriteLine("Service received:\t" + packet.Ethernet.Destination + "\tDISCOVER\txid: " + BitConverter.ToString(receivedDhcp.xid));

                            //--Sending an Dhcp Offer                             
                            sendDhcpOffer(new MacAddress(inter.getHwAddress()), packet.Ethernet.Source, receivedDhcp.xid, receivedDhcp.secs);

                            break;
                        case 0x03:
                            Console.WriteLine("Service received:\t" + packet.Ethernet.Destination + "\tREQUEST\t\txid: " + BitConverter.ToString(receivedDhcp.xid));

                            foreach (DhcpOption item2 in list)
                            {
                                if (item2.optionIdBytes.Equals(0x36))
                                {
                                    if (BitConverter.ToInt32(item2.optionValue, 0).Equals(BitConverter.ToInt32(inter.getIPAddress().GetAddressBytes(), 0)))
                                    {
                                        //--Sending Dhcp Ack  
                                        sendDhcpAck(new MacAddress(inter.getHwAddress()), packet.Ethernet.Source, receivedDhcp.xid, receivedDhcp.secs);
                                    }
                                    else
                                    {
                                        //--Destination prefers other DHCP
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
    }

    public void sendDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tOFFER\t\txid: " + BitConverter.ToString(pTransactionId));
        PacketDevice packetDevice = localhost.getActiveInterface();

        //--Open the output device
        using (PacketCommunicator communicator = packetDevice.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000))
        {
            //--Send down the packet. First build the packet.
            communicator.SendPacket(builder.buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
        }
    }

    public void sendDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tACK\t\txid: " + BitConverter.ToString(pTransactionId));
        PacketDevice packetDevice = localhost.getActiveInterface();

        //--Open the output device
        using (PacketCommunicator communicator = packetDevice.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000))
        {
            //--Send down the packet. First build the packet.
            communicator.SendPacket(builder.buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
        }
    }
}
