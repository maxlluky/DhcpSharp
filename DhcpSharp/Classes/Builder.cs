using DhcpDotNet;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

class Builder
{
    //--Classes
    AddressPool addressPool;
    Interface inter;

    //--Variables
    private IPAddress newClientIPAddress;

    public Builder(AddressPool pAddressPool, Interface pInterface)
    {
        addressPool = pAddressPool;
        inter = pInterface;
    }

    public Packet buildDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        //--Get a new IP from the Pool
        newClientIPAddress = addressPool.getFreeIPAddress();

        //--Get local IP and Subnet
        IPAddress ipaddress = inter.getIPAddress();

        //--Create the DHCP Option
        DHCPv4Option dhcpMessageTypeOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.DhcpMessageType,
            optionLength = 0x01,
            optionValue = new byte[] { 0x02 },
        };

        DHCPv4Option dhcpServerIdentifierOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.ServerIdentifier,
            optionLength = 0x04,
            optionValue = ipaddress.GetAddressBytes(),
        };


        //--DHCP Payload
        DHCPv4Packet dhcpPacket = new DHCPv4Packet
        {
            op = 0x02,
            xid = pTransactionId,
            secs = pSecs,
            ciaddr = new byte[] { 0x00, 0x00, 0x00, 0x00 },
            yiaddr = newClientIPAddress.GetAddressBytes(),
            siaddr = ipaddress.GetAddressBytes(),
            chaddr = PhysicalAddress.Parse(pDestinationMacAddress.ToString().Replace(":", "-")).GetAddressBytes(),
            dhcpOptions = dhcpMessageTypeOption.buildDhcpOption().Concat(dhcpServerIdentifierOption.buildDhcpOption()).ToArray(),
        };

        //--Create the packets layers
        EthernetLayer ethernetLayer = new EthernetLayer
        {
            Source = pSourceMacAddress,
            Destination = pDestinationMacAddress
        };

        IpV4Layer ipV4Layer = new IpV4Layer
        {
            Source = new IpV4Address(ipaddress.ToString()),
            CurrentDestination = new IpV4Address(newClientIPAddress.ToString()),
            Ttl = 128,
        };

        UdpLayer udpLayer = new UdpLayer
        {
            SourcePort = (ushort)67,
            DestinationPort = (ushort)68,
        };

        PayloadLayer payloadLayer = new PayloadLayer
        {
            Data = new Datagram(dhcpPacket.buildPacket()),
        };

        //--Create the builder that will build our packets
        PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

        //--Build the packet
        return builder.Build(DateTime.Now);
    }

    public Packet buildDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        //--Get a new IP from the Pool
        IPAddress ipaddress = inter.getIPAddress();
        IPAddress subnetmask = inter.getNetmask();

        //--Create the DHCP Option
        DHCPv4Option dhcpMessageTypeOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.DhcpMessageType,
            optionLength = 0x01,
            optionValue = new byte[] { 0x05 },
        };

        DHCPv4Option dhcpServerIdentifierOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.ServerIdentifier,
            optionLength = 0x04,
            optionValue = ipaddress.GetAddressBytes(),
        };

        DHCPv4Option ipAddressLeaseTimeOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.IpAddressLeaseTime,
            optionLength = 0x04,
            optionValue = new byte[] { 0x00, 0x0d, 0x2f, 0x00 },
        };

        DHCPv4Option renewalTimeValueOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.RenewalTimeValue,
            optionLength = 0x04,
            optionValue = new byte[] { 0x00, 0x06, 0x97, 0x80 },
        };

        DHCPv4Option rebindTimeValueOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.RebindingTimeValue,
            optionLength = 0x04,
            optionValue = new byte[] { 0x00, 0x0b, 0x89, 0x20 },
        };


        DHCPv4Option subnetMaskOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.Subnetmask,
            optionLength = 0x04,
            optionValue = subnetmask.GetAddressBytes(),
        };

        DHCPv4Option routerOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.Router,
            optionLength = 0x04,
            optionValue = addressPool.getGatewayIpAddress().GetAddressBytes(),
        };

        DHCPv4Option domainNameServerOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.DomainNameServer,
            optionLength = 0x04,
            optionValue = ipaddress.GetAddressBytes(),
        };

        DHCPv4Option domainNameOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.DomainName,
            optionLength = 0x09,
            optionValue = new byte[] { 0x66, 0x72, 0x69, 0x74, 0x7a, 0x2e, 0x62, 0x6f, 0x78 },
        };


        //--DHCP Payload
        DHCPv4Packet dhcpPacket = new DHCPv4Packet
        {
            op = 0x02,
            xid = pTransactionId,
            secs = pSecs,
            ciaddr = new byte[] { 0x00, 0x00, 0x00, 0x00 },
            yiaddr = newClientIPAddress.GetAddressBytes(),
            siaddr = ipaddress.GetAddressBytes(),
            chaddr = PhysicalAddress.Parse(pDestinationMacAddress.ToString().Replace(":", "-")).GetAddressBytes(),
            dhcpOptions = dhcpMessageTypeOption.buildDhcpOption().Concat(dhcpServerIdentifierOption.buildDhcpOption()).Concat(ipAddressLeaseTimeOption.buildDhcpOption()).Concat(renewalTimeValueOption.buildDhcpOption()).Concat(rebindTimeValueOption.buildDhcpOption()).Concat(subnetMaskOption.buildDhcpOption()).Concat(routerOption.buildDhcpOption()).Concat(domainNameServerOption.buildDhcpOption()).Concat(domainNameOption.buildDhcpOption()).ToArray(),
        };

        //--Create the packets layers
        EthernetLayer ethernetLayer = new EthernetLayer
        {
            Source = pSourceMacAddress,
            Destination = pDestinationMacAddress
        };

        IpV4Layer ipV4Layer = new IpV4Layer
        {
            Source = new IpV4Address(ipaddress.ToString()),
            CurrentDestination = new IpV4Address(newClientIPAddress.ToString()),
            Ttl = 128,
        };

        UdpLayer udpLayer = new UdpLayer
        {
            SourcePort = (ushort)67,
            DestinationPort = (ushort)68,
        };

        PayloadLayer payloadLayer = new PayloadLayer
        {
            Data = new Datagram(dhcpPacket.buildPacket()),
        };

        //--Create the builder that will build our packets
        PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

        //--Build the packet
        return builder.Build(DateTime.Now);
    }
}
