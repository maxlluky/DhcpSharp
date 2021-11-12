using PacketDotNet;
using PacketDotNet.DhcpV4;
using SharpPcap;
using System.Net.NetworkInformation;

class DhcpService
{
    private Interface localhost;
    private Config subnetList;
    private PacketBldr builder;
    private ILiveDevice liveDevice;

    public DhcpService(Interface pLocalhost, Config pSubnetList)
    {
        localhost = pLocalhost;
        subnetList = pSubnetList;
    }

    public void startListen()
    {
        liveDevice = localhost.getActiveInterface();
        liveDevice.Open(DeviceModes.Promiscuous, 1000);

        Console.WriteLine("Listening on {0} - Using ({1}) Subnet-configurations!\n", liveDevice.Description, subnetList.subnetList.Count);
        Console.WriteLine("Status\t\tDestination MAC\t\tMessage\t\tTransaction ID\t\tSubnet (Range)");
        Console.WriteLine("===================================================================================================================");

        builder = new PacketBldr();

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
            DhcpV4Packet dhcpv4Packet = packet.Extract<DhcpV4Packet>();


            if (udpPacket != null)
            {
                if (udpPacket.SourcePort == 68 & udpPacket.DestinationPort == 67)
                {
                    foreach (DhcpV4Option dhcpv4Option in dhcpv4Packet.GetOptions())
                    {
                        if (dhcpv4Option.OptionType == DhcpV4OptionType.DHCPMsgType)
                        {
                            switch (dhcpv4Option.Data[0])
                            {
                                //--Packet is a Discover
                                case 0x01:
                                    //--Check Subnet-Config
                                    foreach (Subnet subnet in subnetList.subnetList)
                                    {
                                        if (ipv4Packet.SourceAddress.ToString() == subnet.listenIp.ToString())
                                        {
                                            //--Match found! Response to VLAN now.                                           
                                            sendDhcpOffer(liveDevice.MacAddress, dhcpv4Packet.ClientHardwareAddress, dhcpv4Packet.TransactionId, dhcpv4Packet.Secs, subnet);
                                        }
                                    }
                                    break;

                                //--Packet is an Request
                                case 0x03:
                                    //--Check Subnet-Config                                    
                                    foreach (Subnet subnet in subnetList.subnetList)
                                    {
                                        if (ipv4Packet.SourceAddress.ToString() == subnet.listenIp.ToString())
                                        {
                                            //--Match found. Response to VLAN now!
                                            sendDhcpResponse(liveDevice.MacAddress, dhcpv4Packet.ClientHardwareAddress, dhcpv4Packet.TransactionId, dhcpv4Packet.Secs, subnet);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception) { }
    }

    public void sendDhcpOffer(PhysicalAddress pSourceMacAddress, PhysicalAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
    {
        Console.WriteLine("Service send\t{0}\t\tOFFER\t\t{1}\t\t{2} - {3}", pDestinationMacAddress, pTransactionId, pSubnet.rangeStartIp, pSubnet.rangeEndIp);
        liveDevice.SendPacket(builder.buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs, pSubnet));
    }

    public void sendDhcpResponse(PhysicalAddress pSourceMacAddress, PhysicalAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
    {
        Console.WriteLine("Service send\t{0}\t\tACK\t\t{1}", pDestinationMacAddress, pTransactionId);
        liveDevice.SendPacket(builder.buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs, pSubnet));
    }
}
