using DhcpDotNet;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class DhcpService
{
    private Interface local_interface;
    private Config subnet_conf;
    private NetPacket netPckt_bldr;
    private PacketDevice liveDevice;
    private PacketCommunicator packetCommunicator;

    public DhcpService(Interface pLocalhost, Config pSubnetList)
    {
        local_interface = pLocalhost;
        subnet_conf = pSubnetList;
    }

    public void startListen()
    {
        liveDevice = local_interface.getActiveInterface();
        packetCommunicator = liveDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);

        Console.WriteLine("Status\t\t\tDestination MAC\t\tDHCP Message\tTransaction ID\t\tServer Identifier");
        Console.WriteLine("===========================================================================================================");

        netPckt_bldr = new NetPacket();

        packetCommunicator.SetFilter("udp");
        packetCommunicator.ReceivePackets(0, receiveCallback);

        Debug.WriteLine("Service started...");
    }

    private void receiveCallback(Packet packet)
    {
        try
        {
            IpV4Datagram ipv4Packet = packet.Ethernet.IpV4;
            UdpDatagram udpPacket = ipv4Packet.Udp;
            Datagram datagram = udpPacket.Payload;

            if (udpPacket.SourcePort.Equals(68) & udpPacket.DestinationPort.Equals(67))
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

                                    foreach (Subnet subnet in subnet_conf.subnetList)
                                    {
                                        if (ipv4Packet.Source.ToString() == subnet.listenIp.ToString())
                                        {
                                            //--Match found! Response to VLAN now.                                           
                                            sendDhcpOffer(local_interface.getMacAddress(), convertMacAddress(dhcpv4Packet.chaddr), BitConverter.ToUInt32(dhcpv4Packet.xid, 0), BitConverter.ToUInt16(dhcpv4Packet.secs, 0), subnet);
                                        }
                                    }


                                    break;
                                //--Packet is an Request
                                case 0x03:

                                    //--Check Subnet-Config                                    
                                    foreach (Subnet subnet in subnet_conf.subnetList)
                                    {
                                        if (ipv4Packet.Source.ToString() == subnet.listenIp.ToString())
                                        {
                                            //--Match found. Response to VLAN now!                                            
                                            sendDhcpResponse(local_interface.getMacAddress(), convertMacAddress(dhcpv4Packet.chaddr), BitConverter.ToUInt32(dhcpv4Packet.xid, 0), BitConverter.ToUInt16(dhcpv4Packet.secs, 0), subnet);
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

    private MacAddress convertMacAddress(byte[] pMacBytes)
    {

        return new MacAddress();
    }

    public void sendDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
    {
        Console.WriteLine("Service send\t{0}\t\tOFFER\t\t{1}\t\t{2} - {3}", pDestinationMacAddress, pTransactionId, pSubnet.rangeStartIp, pSubnet.rangeEndIp);
        packetCommunicator.SendPacket(netPckt_bldr.buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs, pSubnet));
    }

    public void sendDhcpResponse(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
    {
        Console.WriteLine("Service send\t{0}\t\tACK\t\t{1}", pDestinationMacAddress, pTransactionId);
        packetCommunicator.SendPacket(netPckt_bldr.buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs, pSubnet));
    }
}
