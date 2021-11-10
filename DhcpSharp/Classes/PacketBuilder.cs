using PacketDotNet;
using PacketDotNet.DhcpV4;
using PacketDotNet.Utils;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

class PacketBuilder
{
    private List<Client> clientList = new List<Client>();

    public Packet buildDhcpOffer(PhysicalAddress pSourceMacAddress, PhysicalAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
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

        //--Build Eth, IP, UDP        
        EthernetPacket ethernetPacket = new EthernetPacket(pSourceMacAddress, pDestinationMacAddress, EthernetType.IPv4);
        IPv4Packet ipv4Packet = new IPv4Packet(dhcpIp, newClientIPAddress);
        UdpPacket udpPacket = new UdpPacket(68, 67);

        //--Build DHCP
        IList<DhcpV4Option> dhcpOptionList = new List<DhcpV4Option>();
        dhcpOptionList.Add(new MessageTypeOption(DhcpV4MessageType.Offer));
        dhcpOptionList.Add(new ServerIdOption(dhcpIp));
        dhcpOptionList.Add(new DomainNameServerOption(IPAddress.Parse(pSubnet.dnsIp)));
        dhcpOptionList.Add(new DomainNameOption(pSubnet.domainName));

        DhcpV4Packet dhcpv4Packet = new DhcpV4Packet(new ByteArraySegment(new byte[300]), udpPacket)
        {
            MessageType = DhcpV4MessageType.Offer,
            Operation = DhcpV4Operation.BootReply,
            HardwareType = DhcpV4HardwareType.Ethernet,
            HardwareLength = 0x06,
            Xid = pTransactionId,
            Secs = pSecs,
            ClientAddress = IPAddress.Parse("0.0.0.0"),
            YourAddress = newClientIPAddress,
            ServerAddress = dhcpIp,
            ClientHardwareAddress = pDestinationMacAddress,
            MagicNumber = 1669485411,            
        };

        dhcpv4Packet.SetOptions(dhcpOptionList);

        //--Checksum
        Debug.WriteLine("Calculating now Checksum");
        udpPacket.Checksum = calculateUdpChecksum(dhcpIp, newClientIPAddress, udpPacket);
        ipv4Packet.Checksum = 65535;

        //--Merge
        udpPacket.PayloadData = dhcpv4Packet.Bytes;
        ipv4Packet.PayloadPacket = udpPacket;
        ethernetPacket.PayloadPacket = ipv4Packet;

        return ethernetPacket;
    }

    private ushort calculateUdpChecksum(IPAddress pSourceIp, IPAddress pDestIp, UdpPacket pUdpPacket)
    {
        ushort checksum = 0;
        byte[] checksumBytes;

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                Debug.WriteLine("SourceIp: {0}", pSourceIp.ToString());
                Debug.WriteLine("DestIp: {0}", pDestIp.ToString());
                Debug.WriteLine("Length: {0}", Convert.ToInt16(pUdpPacket.Length));
                Debug.WriteLine("S-Port: {0}", pUdpPacket.SourcePort);
                Debug.WriteLine("D-Port: {0}", pUdpPacket.DestinationPort);

                binaryWriter.Write(pSourceIp.GetAddressBytes()); //--32-bit Source IP
                binaryWriter.Write(pDestIp.GetAddressBytes()); //--32-bit Dest IP
                binaryWriter.Write((ushort)17); //--16-bit Zeros and Protocol
                binaryWriter.Write(Convert.ToInt16(pUdpPacket.Length)); //--16-bit UDP length
                binaryWriter.Write(pUdpPacket.SourcePort); //--16bit Source Port
                binaryWriter.Write(pUdpPacket.DestinationPort); //--16bit Dest Port
                binaryWriter.Write(Convert.ToInt16(pUdpPacket.TotalPacketLength)); //--16-bit UDP length
                // binaryWriter.Write(pUdpPacket.PayloadData); //--Data
            }

            checksumBytes = memoryStream.ToArray();
        }

        Debug.WriteLine("Got ChecksumBytes");
        /*
        for (int i=0; i<checksumBytes.Length; i++)
        {
            checksum += Convert.ToByte(checksumBytes[i] % 256);
        }
        */

        for (int i=0; i<checksumBytes.Length; i++)
        {
            byte[] sixteen = { checksumBytes[i], checksumBytes[i++] };
            checksum += BitConverter.ToUInt16(sixteen, 0);           
        }            

        Debug.WriteLine("Calculated Checksum: {0}", checksum);
        return checksum;
    }

    public Packet buildDhcpAck(PhysicalAddress pSourceMacAddress, PhysicalAddress pDestinationMacAddress, uint pTransactionId, ushort pSecs, Subnet pSubnet)
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

        //--Build Eth, IP, UDP
        EthernetPacket ethernetPacket = new EthernetPacket(pSourceMacAddress, pDestinationMacAddress, EthernetType.IPv4);
        IPv4Packet ipv4Packet = new IPv4Packet(dhcpIp, newClientIPAddress);
        UdpPacket udpPacket = new UdpPacket(67, 68);

        //--Build DHCP
        DhcpV4Packet dhcpv4Packet = new DhcpV4Packet(new ByteArraySegment(new byte[350]), null)
        {
            MessageType = DhcpV4MessageType.Ack,
            Operation = DhcpV4Operation.BootReply,
            HardwareType = DhcpV4HardwareType.Ethernet,
            HardwareLength = 0x06,
            Xid = pTransactionId,
            Secs = pSecs,
            ClientAddress = IPAddress.Parse("0.0.0.0"),
            YourAddress = newClientIPAddress,
            ServerAddress = dhcpIp,
            ClientHardwareAddress = pDestinationMacAddress,
            MagicNumber = 1669485411,
        };

        IList<DhcpV4Option> dhcpOptionList = new List<DhcpV4Option>();
        dhcpOptionList.Add(new MessageTypeOption(DhcpV4MessageType.Ack));
        dhcpOptionList.Add(new ServerIdOption(dhcpIp));
        dhcpOptionList.Add(new AddressTimeOption(TimeSpan.FromSeconds(864000)));
        dhcpOptionList.Add(new RenewalTimeOption(TimeSpan.FromSeconds(432000)));
        dhcpOptionList.Add(new RebindingTimeOption(TimeSpan.FromSeconds(756000)));
        dhcpOptionList.Add(new SubnetMaskOption(IPAddress.Parse(pSubnet.netmask)));
        dhcpOptionList.Add(new RouterOption(IPAddress.Parse(pSubnet.gatewayIp)));
        dhcpOptionList.Add(new DomainNameServerOption(IPAddress.Parse(pSubnet.dnsIp)));
        dhcpOptionList.Add(new DomainNameOption(pSubnet.domainName));
        dhcpv4Packet.SetOptions(dhcpOptionList);

        //--Merge
        udpPacket.PayloadPacket = dhcpv4Packet;
        ipv4Packet.PayloadPacket = udpPacket;
        ethernetPacket.PayloadPacket = ipv4Packet;

        return ethernetPacket;
    }
}
