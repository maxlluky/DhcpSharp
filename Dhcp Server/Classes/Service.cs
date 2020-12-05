using DhcpDotNet;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

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
            Console.WriteLine("Status\t\t\tDestination MAC\t\tDHCP Message\tTransaction ID");
            Console.WriteLine("===================================================================================");

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
            //--Send down the packet
            communicator.SendPacket(buildDhcpOffer(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
        }
    }

    public void sendDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        Console.WriteLine("Service send:\t\t" + pDestinationMacAddress + "\tACK\t\txid: " + BitConverter.ToString(pTransactionId));
        PacketDevice packetDevice = localhost.getActiveInterface();

        //--Open the output device
        using (PacketCommunicator communicator = packetDevice.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000))
        {
            //--Send down the packet
            communicator.SendPacket(buildDhcpAck(pSourceMacAddress, pDestinationMacAddress, pTransactionId, pSecs));
        }
    }

    private Packet buildDhcpOffer(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        //--Get a new IP from the Pool
        newClientIPAddress = addressPool.getFreeIPAddress();

        //--Get local IP and Subnet
        IPAddress ipaddress = inter.getIPAddress();

        //--Create the DHCP Option
        DhcpOption dhcpMessageTypeOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DhcpMessageType,
            optionLength = 0x01,
            optionValue = new byte[] { 0x02 },
        };

        DhcpOption dhcpServerIdentifierOption = new DhcpOption
        {
            optionId = dhcpOptionIds.ServerIdentifier,
            optionLength =  0x04,
            optionValue = ipaddress.GetAddressBytes(),
        };
      

        //--DHCP Payload
        DhcpPacket dhcpPacket = new DhcpPacket
        {
            op = new byte[] { 0x02 },
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

    private Packet buildDhcpAck(MacAddress pSourceMacAddress, MacAddress pDestinationMacAddress, byte[] pTransactionId, byte[] pSecs)
    {
        //--Get a new IP from the Pool
        IPAddress ipaddress = inter.getIPAddress();
        IPAddress subnetmask = inter.getNetmask();

        //--Create the DHCP Option
        DhcpOption dhcpMessageTypeOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DhcpMessageType,
            optionLength = 0x01,
            optionValue = new byte[] { 0x05 },
        };

        DhcpOption dhcpServerIdentifierOption = new DhcpOption
        {
            optionId = dhcpOptionIds.ServerIdentifier,
            optionLength = 0x04,
            optionValue = ipaddress.GetAddressBytes(),
        };

        DhcpOption ipAddressLeaseTimeOption = new DhcpOption
        {
            optionId = dhcpOptionIds.IpAddressLeaseTime,
            optionLength = 0x04,
            optionValue = new byte[] { 0x00, 0x0d, 0x2f, 0x00 },
        };

        DhcpOption renewalTimeValueOption = new DhcpOption
        {
            optionId = dhcpOptionIds.RenewalTimeValue,
            optionLength = 0x04,
            optionValue = new byte[] { 0x00, 0x06, 0x97, 0x80 },
        };

        DhcpOption rebindTimeValueOption = new DhcpOption
        {
            optionId = dhcpOptionIds.RebindingTimeValue,
            optionLength = 0x04,
            optionValue = new byte[] { 0x00, 0x0b, 0x89, 0x20 },
        };


        DhcpOption subnetMaskOption = new DhcpOption
        {
            optionId = dhcpOptionIds.Subnetmask,
            optionLength = 0x04,
            optionValue = subnetmask.GetAddressBytes(),
        };

        DhcpOption routerOption = new DhcpOption
        {
            optionId = dhcpOptionIds.Router,
            optionLength = 0x04,
            optionValue = addressPool.getGatewayIpAddress().GetAddressBytes(),
        };

        DhcpOption domainNameServerOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DomainNameServer,
            optionLength = 0x04,
            optionValue = ipaddress.GetAddressBytes(),
        };

        DhcpOption domainNameOption = new DhcpOption
        {
            optionId = dhcpOptionIds.DomainName,
            optionLength = 0x09,
            optionValue = new byte[] { 0x66, 0x72, 0x69, 0x74, 0x7a, 0x2e, 0x62, 0x6f, 0x78 },
        };


        //--DHCP Payload
        DhcpPacket dhcpPacket = new DhcpPacket
        {
            op = new byte[] { 0x02 },
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
