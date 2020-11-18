using DhcpDotNet;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

class Service
{
    //--Classes
    private static Localhost localhost;
    private readonly Interface inter;
    private AddressPool addressPool;

    private IPAddress newClientIPAddress;

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

        //--Create a new DhcpPacket to parse the received and read Data from it.
        DhcpPacket receivedDhcp = new DhcpPacket();
        receivedDhcp.parsePacket(datagram.ToArray());

        //--Check if Packet is DHCP-Discover (= 300 Bytes). Comming soon e.g. parsing
        if (ipPacket.Source.ToString() == "0.0.0.0" & ipPacket.Destination.ToString() == "255.255.255.255" & udpDatagram.SourcePort == (ushort)68 & udpDatagram.DestinationPort == (ushort)67)
        {
            //--Sending an Dhcp Offer 
            sendDhcpOffer(new MacAddress(inter.getHwAddress()), packet.Ethernet.Source, receivedDhcp.transactionID);

            //--Temp. waiting for dhcp request
            Thread.Sleep(500);

            //--Sending Dhcp Ack
            sendDhcpAck(new MacAddress(inter.getHwAddress()), packet.Ethernet.Source, receivedDhcp.transactionID);
        }
    }

    private Packet buildDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId)
    {
        //--Get a new IP from the Pool
        newClientIPAddress = addressPool.getFreeIPAddress();

        //--Get local IP and Subnet
        IPAddress ipaddress = inter.getIPAddress();
        IPAddress subnetmask = inter.getNetmask();

        //--Create the DHCP Option
        DhcpOption dhcpMessageTypeOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DhcpMessageType,
            optionLength = new byte[] { 0x01 },
            optionValue = new byte[] { 0x02 },
        };

        DhcpOption dhcpServerIdentifierOption = new DhcpOption
        {
            optionId = dhcpOptionIds.ServerIdentifier,
            optionLength = new byte[] { 0x04 },
            optionValue = ipaddress.GetAddressBytes(),
        };

        DhcpOption ipAddressLeaseTimeOption = new DhcpOption
        {
            optionId = dhcpOptionIds.IpAddressLeaseTime,
            optionLength = new byte[] { 0x04 },
            optionValue = new byte[] { 0x00, 0x0d, 0x2f, 0x00 },
        };

        DhcpOption renewalTimeValueOption = new DhcpOption
        {
            optionId = dhcpOptionIds.RenewalTimeValue,
            optionLength = new byte[] { 0x04 },
            optionValue = new byte[] { 0x00, 0x06, 0x97, 0x80 },
        };

        DhcpOption rebindTimeValueOption = new DhcpOption
        {
            optionId = dhcpOptionIds.RebindingTimeValue,
            optionLength = new byte[] { 0x04 },
            optionValue = new byte[] { 0x00, 0x0b, 0x89, 0x20 },
        };

        DhcpOption subnetMaskOption = new DhcpOption
        {
            optionId = dhcpOptionIds.Subnetmask,
            optionLength = new byte[] { 0x04 },
            optionValue = subnetmask.GetAddressBytes(),
        };

        DhcpOption routerOption = new DhcpOption
        {
            optionId = dhcpOptionIds.Router,
            optionLength = new byte[] { 0x04 },
            optionValue = ipaddress.GetAddressBytes(),
        };

        DhcpOption domainNameServerOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DomainNameServer,
            optionLength = new byte[] { 0x04 },
            optionValue = ipaddress.GetAddressBytes(),
        };

        DhcpOption domainNameOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DomainName,
            optionLength = new byte[] { 0x09 },
            optionValue = new byte[] { 0x66, 0x72, 0x69, 0x74, 0x7a, 0x2e, 0x62, 0x6f, 0x78 },
        };

        //--DHCP Payload
        DhcpPacket dhcpPacket = new DhcpPacket
        {
            messageType = new byte[] { 0x02 },
            transactionID = pTransactionId,
            clientIP = new byte[] { 0x00, 0x00, 0x00, 0x00 },
            yourIP = newClientIPAddress.GetAddressBytes(),
            nextServerIP = ipaddress.GetAddressBytes(),
            clientMac = PhysicalAddress.Parse(pDestinationMacAddress.ToString().Replace(":", "-")).GetAddressBytes(),
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

    private Packet buildDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId)
    {
        //--Get a new IP from the Pool
        IPAddress ipaddress = inter.getIPAddress();

        //--Create the DHCP Option
        DhcpOption dhcpMessageTypeOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DhcpMessageType,
            optionLength = new byte[] { 0x01 },
            optionValue = new byte[] { 0x05 },
        };

        DhcpOption dhcpServerIdentifierOption = new DhcpOption
        {
            optionId = dhcpOptionIds.ServerIdentifier,
            optionLength = new byte[] { 0x04 },
            optionValue = ipaddress.GetAddressBytes(),
        };

        //--DHCP Payload
        DhcpPacket dhcpPacket = new DhcpPacket
        {
            messageType = new byte[] { 0x02 },
            transactionID = pTransactionId,
            clientIP = new byte[] { 0x00, 0x00, 0x00, 0x00 },
            yourIP = newClientIPAddress.GetAddressBytes(),
            nextServerIP = ipaddress.GetAddressBytes(),
            clientMac = PhysicalAddress.Parse(pDestinationMacAddress.ToString().Replace(":", "-")).GetAddressBytes(),
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

    public void sendDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId)
    {
        Console.WriteLine("Sending DHCP-Offer...\t " + pDestinationMacAddress);

        PacketDevice packetDevice = localhost.getActiveInterface();

        //--Open the output device
        using (PacketCommunicator communicator = packetDevice.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000))
        {
            //--Send down the packet
            communicator.SendPacket(buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId));
        }
    }

    public void sendDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId)
    {
        Console.WriteLine("Sending DHCP-ACK...\t " + pDestinationMacAddress);

        PacketDevice packetDevice = localhost.getActiveInterface();

        //--Open the output device
        using (PacketCommunicator communicator = packetDevice.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000))
        {
            //--Send down the packet
            communicator.SendPacket(buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId));
        }
    }
}
