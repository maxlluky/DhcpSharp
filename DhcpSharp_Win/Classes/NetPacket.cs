using DhcpDotNet;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

class NetPacket
{
    private List<Client> clientList = new List<Client>();

    public Packet buildDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
    {
        IPAddress newClientIPAddress = IPAddress.Parse("0.0.0.0");
        bool clientFound = false;

        if (clientList.Count != 0)
        {
            foreach (Client item in clientList)
            {
                if (StructuralComparisons.StructuralEqualityComparer.Equals(item.macAddress, pDestinationMacAddress))
                {
                    newClientIPAddress = item.ipaddress;
                    clientFound = true;

                    Debug.WriteLine("Listed Client has been found with IP-Binding: " + newClientIPAddress.ToString());
                }
            }
        }


        if (!clientFound)
        {
            newClientIPAddress = pSubnet.getFreeIPAddress();
            Client client = new Client(newClientIPAddress.ToString(), newClientIPAddress, pTransactionId, pDestinationMacAddress);
            clientList.Add(client);

            Debug.WriteLine("Listed Client has been generated with IP-Binding: " + newClientIPAddress.ToString() + " and xid: " + pTransactionId + " and MacAddress: " + pDestinationMacAddress.ToString());
        }

        IPAddress dhcpIp = IPAddress.Parse(pSubnet.dhcpIp);

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
            optionValue = dhcpIp.GetAddressBytes(),
        };

        DHCPv4Packet dhcpPacket = new DHCPv4Packet
        {
            op = 0x02,
            htype = 0x01,
            hlen = 0x06,
            xid = BitConverter.GetBytes(pTransactionId),
            secs = BitConverter.GetBytes(pSecs),
            ciaddr = new byte[] { 0x00, 0x00, 0x00, 0x00 },
            yiaddr = newClientIPAddress.GetAddressBytes(),
            siaddr = dhcpIp.GetAddressBytes(),
            chaddr = PhysicalAddress.Parse(pDestinationMacAddress.ToString().Replace(":", "-")).GetAddressBytes(),
            dhcpOptions = dhcpMessageTypeOption.buildDhcpOption().Concat(dhcpServerIdentifierOption.buildDhcpOption()).ToArray(),
        };

        EthernetLayer ethernetLayer = new EthernetLayer
        {
            Source = pSourceMacAddress,
            Destination = pDestinationMacAddress
        };

        IpV4Layer ipV4Layer = new IpV4Layer
        {
            Source = new IpV4Address(dhcpIp.ToString()),
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

        PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);
        return builder.Build(DateTime.Now);
    }

    public Packet buildDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
    {
        IPAddress newClientIPAddress = IPAddress.Parse("0.0.0.0");
        bool clientFound = false;

        if (clientList.Count != 0)
        {
            foreach (Client item in clientList)
            {
                if (StructuralComparisons.StructuralEqualityComparer.Equals(item.macAddress, pDestinationMacAddress))
                {

                    newClientIPAddress = item.ipaddress;
                    clientFound = true;

                    Debug.WriteLine("Listed Client has been found with IP-Binding: " + newClientIPAddress.ToString());
                }
            }
        }

        if (!clientFound)
        {
            Debug.WriteLine("Server sends a NAK. There is no TransactionId paired to a leased IP-Address! The Server did not received a DISCOVER from the Client");
        }

        IPAddress dhcpIp = IPAddress.Parse(pSubnet.dhcpIp);
        IPAddress dnsIp = IPAddress.Parse(pSubnet.dnsIp);
        IPAddress gateway = IPAddress.Parse(pSubnet.gatewayIp);
        IPAddress subnetmask = IPAddress.Parse(pSubnet.netmask);

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
            optionValue = dhcpIp.GetAddressBytes(),
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
            optionValue = gateway.GetAddressBytes(),
        };

        DHCPv4Option domainNameServerOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.DomainNameServer,
            optionLength = 0x04,
            optionValue = dnsIp.GetAddressBytes(),
        };

        DHCPv4Option domainNameOption = new DHCPv4Option
        {
            optionId = DHCPv4OptionIds.DomainName,
            optionLength = (byte)pSubnet.domainName.Length,
            optionValue = Encoding.ASCII.GetBytes(pSubnet.domainName),
        };

        DHCPv4Packet dhcpPacket = new DHCPv4Packet
        {
            op = 0x02,
            htype = 0x01,
            hlen = 0x06,
            xid = BitConverter.GetBytes(pTransactionId),
            secs = BitConverter.GetBytes(pSecs),
            ciaddr = new byte[] { 0x00, 0x00, 0x00, 0x00 },
            yiaddr = newClientIPAddress.GetAddressBytes(),
            siaddr = dhcpIp.GetAddressBytes(),
            chaddr = PhysicalAddress.Parse(pDestinationMacAddress.ToString().Replace(":", "-")).GetAddressBytes(),
            dhcpOptions = dhcpMessageTypeOption.buildDhcpOption().Concat(dhcpServerIdentifierOption.buildDhcpOption()).Concat(ipAddressLeaseTimeOption.buildDhcpOption()).Concat(renewalTimeValueOption.buildDhcpOption()).Concat(rebindTimeValueOption.buildDhcpOption()).Concat(subnetMaskOption.buildDhcpOption()).Concat(routerOption.buildDhcpOption()).Concat(domainNameServerOption.buildDhcpOption()).Concat(domainNameOption.buildDhcpOption()).ToArray(),
        };

        EthernetLayer ethernetLayer = new EthernetLayer
        {
            Source = pSourceMacAddress,
            Destination = pDestinationMacAddress
        };

        IpV4Layer ipV4Layer = new IpV4Layer
        {
            Source = new IpV4Address(dhcpIp.ToString()),
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

        PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);
        return builder.Build(DateTime.Now);
    }
}
