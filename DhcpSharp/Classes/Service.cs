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
    private static Localhost localhost;
    private readonly Interface inter;
    private AddressPool addressPool;
    private Builder builder;
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
        packetDevice = localhost.getActiveInterface();
        packetCommunicator = packetDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);
        
        Console.WriteLine("Listening on " + packetDevice.Description + "...");
        Console.WriteLine("Status\t\t\tDestination MAC\t\tDHCP Message\tTransaction ID\t\tServer Identifier");
        Console.WriteLine("===========================================================================================================");
        
        builder = new Builder(addressPool, inter);
        packetCommunicator.SetFilter("udp");
        packetCommunicator.ReceivePackets(0, receiveCallback);
    }

    private void receiveCallback(Packet packet)
    {
        try
        {
            IpV4Datagram ipPacket = packet.Ethernet.IpV4;
            UdpDatagram udpDatagram = ipPacket.Udp;
            Datagram datagram = udpDatagram.Payload;
            
            if (udpDatagram.SourcePort.Equals(68) & udpDatagram.DestinationPort.Equals(67))
            {
                DHCPv4Packet dhcpv4Packet = new DHCPv4Packet();
                if (dhcpv4Packet.parsePacket(datagram.ToArray()))
                {          
                    List<DHCPv4Option> list = new DHCPv4Option().parseDhcpOptions(dhcpv4Packet.dhcpOptions);
                    foreach (DHCPv4Option dhcpMessageTypeOption in list)
                    {                        
                        if (dhcpMessageTypeOption.optionIdBytes.Equals(0x35))
                        {
                            switch (dhcpMessageTypeOption.optionValue[0])
                            {
                                //--Packet is a Discover
                                case 0x01:
                                    Console.WriteLine("Service received:\t" + packet.Ethernet.Destination + "\tDISCOVER\txid: " + BitConverter.ToString(dhcpv4Packet.xid));                        
                                    sendDhcpOffer(new MacAddress(inter.getMacAddress()), packet.Ethernet.Source, dhcpv4Packet.xid, dhcpv4Packet.secs);

                                    break;
                                //--Packet is an Request
                                case 0x03:
                                    foreach (DHCPv4Option dhcpServerIdentifierOption in list)
                                    {
                                        //--DHCP contains Server-Identifier-Option.
                                        if (dhcpServerIdentifierOption.optionIdBytes.Equals(0x36))
                                        {
                                            //--DHCP-Server-Identifier equals IP-Address of DHCP-Server.
                                            if (BitConverter.ToInt32(dhcpServerIdentifierOption.optionValue, 0).Equals(BitConverter.ToInt32(inter.getIPAddress().GetAddressBytes(), 0)))
                                            {
                                                Console.WriteLine("Service received:\t" + packet.Ethernet.Destination + "\tREQUEST\t\txid: " + BitConverter.ToString(dhcpv4Packet.xid) + "\tSID: " + BitConverter.ToString(dhcpServerIdentifierOption.optionValue, 0));
                                                sendDhcpAck(new MacAddress(inter.getMacAddress()), packet.Ethernet.Source, dhcpv4Packet.xid, dhcpv4Packet.secs);
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
        packetCommunicator.SendPacket(builder.buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
    }

    public void sendDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tACK\t\txid: " + BitConverter.ToString(pTransactionId));
        packetCommunicator.SendPacket(builder.buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
    }
}
