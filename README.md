# DhcpSharp
DhcpShar is a minimal DHCPv4 server based on DhcpDotNet and Pcap.NET. The server supports a simple configuration of the gateway and the address pool. The programme only supports the normal DHCPv4 handshake used for assigning IPv4 addresses. Additional functions such as the renewal of a lease are not implemented.

## See how its done
Inside the Wireshark folder is an IPv4 and IPv6 .pcap-example of what a Dhcp process can look like. For simplicity, DhcpSharp was reconstructed on the basis of the IPv4-example. The library "DhcpDotNet" is used to create the DHCP packets. In addition, the library "Pcap.NET" is used to encapsulate the DHCP packets and transmit them via the network interface.

## Coming soon
I´m planning to implement the DHCP lease time mechanism in the near future, allowing clients to be reassigned an IP address after the lease time has expired. I also want to use DhcpSharp with the SharpPcap library in the future to improve performance, understandability and consistency.

## Copyright
The contents and works in this software created by the software operators are subject to German copyright law. The reproduction, editing, distribution and any kind of use outside the limits of copyright law require the written consent of the respective author or creator. Downloads and copies of this software are only permitted for private, non-commercial use.

Insofar as the content on this software was not created by the operator, the copyrights of third parties are observed. In particular, third-party content is identified as such. Should you nevertheless become aware of a copyright infringement, please inform us accordingly. If we become aware of any infringements, we will remove such contents immediately.

Source: [eRecht24.de](https://www.e-recht24.de/)
