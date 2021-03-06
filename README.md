# DhcpSharp
Minimal DHCPv4 server based on DhcpDotNet and Pcap.NET. Supports easy configuration of the gateway and address pool. The service only supports the normal DHCPv4 handshake, which is used for the allocation of IPv4 addresses. Extras such as renewing a lease are not implemented.

## See how its done
In the "Wireshark" folder there are two examples for IPv4 and IPv6 of how a Dhcp process can look like. To put it simply, DhcpSharp was reconstructed on the basis of these examples. The library "DhcpDotNet" was used to create the DHCP-Packets. In addition the library "Pcap.NET" was used to encapsulate the DHCP Packets and send them down.

## Coming soon
I am planning to implement the DHCP lease time mechanism in the near future, allowing clients to be reassigned an IP address after the lease time has expired. 

## Copyright
The contents and works in this software created by the software operators are subject to German copyright law. The reproduction, editing, distribution and any kind of use outside the limits of copyright law require the written consent of the respective author or creator. Downloads and copies of this software are only permitted for private, non-commercial use.

Insofar as the content on this software was not created by the operator, the copyrights of third parties are observed. In particular, third-party content is identified as such. Should you nevertheless become aware of a copyright infringement, please inform us accordingly. If we become aware of any infringements, we will remove such contents immediately.

Source: [eRecht24.de](https://www.e-recht24.de/)
