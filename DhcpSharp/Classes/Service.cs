using DhcpDotNet;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

class Service
{
    private static Localhost localhost;
    private readonly Interface inter;
    private SubnetList subnetList;
    private Builder builder;
    private ILiveDevice liveDevice;

    public Service(Localhost pLocalhost, Interface pInter, SubnetList pSubnetList)
    {
        localhost = pLocalhost;
        inter = pInter;
        subnetList = pSubnetList;
    }

    public void startListen()
    {
        liveDevice = localhost.getActiveInterface();
        liveDevice.Open(DeviceModes.Promiscuous, 1000);

        Console.WriteLine("Listening on " + liveDevice.Description + "...");
        Console.WriteLine("Status\t\t\tDestination MAC\t\tDHCP Message\tTransaction ID\t\tServer Identifier");
        Console.WriteLine("===========================================================================================================");

        builder = new Builder(inter);

        liveDevice.OnPacketArrival +=
           new PacketArrivalEventHandler(device_OnPacketArrival);

        liveDevice.StartCapture();
    }

    private void device_OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var rawPacket = e.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            EthernetPacket ethernetPacket = packet.Extract<EthernetPacket>();
            IPv4Packet ipv4Packet = packet.Extract<IPv4Packet>();
            UdpPacket udpPacket = packet.Extract<UdpPacket>();
            byte[] payload = udpPacket.PayloadData;

            if (udpPacket.SourcePort.Equals(68) & udpPacket.DestinationPort.Equals(67))
            {
                DHCPv4Packet dhcpv4Packet = new DHCPv4Packet();
                if (dhcpv4Packet.parsePacket(payload))
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
                                    //--Check Subnet-Config
                                    foreach (Subnet subnet in subnetList.list)
                                    {
                                        if (ipv4Packet.SourceAddress.ToString() == subnet.gatewayIp.ToString())
                                        {
                                            //--Match found. Response to VLAN now!
                                            sendDhcpOffer(PhysicalAddress.Parse(inter.getMacAddress()), new PhysicalAddress(dhcpv4Packet.chaddr), dhcpv4Packet.xid, dhcpv4Packet.secs, subnet);
                                        }
                                    }
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
                                                //--Check Subnet-Config
                                                foreach (Subnet subnet in subnetList.list)
                                                {
                                                    if (ipv4Packet.SourceAddress.ToString() == subnet.gatewayIp.ToString())
                                                    {
                                                        //--Match found. Response to VLAN now!
                                                        sendDhcpAck(PhysicalAddress.Parse(inter.getMacAddress()), new PhysicalAddress(dhcpv4Packet.chaddr), dhcpv4Packet.xid, dhcpv4Packet.secs, subnet);
                                                    }
                                                }
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

    public void sendDhcpOffer(PhysicalAddress pSourceMacAddress, PhysicalAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs, Subnet pSubnet)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tOFFER\t\txid: " + BitConverter.ToString(pTransactionId));
        liveDevice.SendPacket(builder.buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs, pSubnet));
    }

    public void sendDhcpAck(PhysicalAddress pSourceMacAddress, PhysicalAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs, Subnet pSubnet)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tACK\t\txid: " + BitConverter.ToString(pTransactionId));
        liveDevice.SendPacket(builder.buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs, pSubnet));
    }
}
