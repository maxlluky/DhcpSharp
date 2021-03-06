﻿using PcapDotNet.Packets.Ethernet;
using System.Net;
class Client
{
    public string hostname { get; set; }
    public IPAddress ipaddress { get; set; }
    public byte[] transactionId { get; set; }
    public MacAddress macAddress { get; set; }

    public Client(string pHostname, IPAddress pIPAddress, byte[] pTransactionId, MacAddress pMacAddress)
    {
        hostname = pHostname;
        ipaddress = pIPAddress;
        transactionId = pTransactionId;
        macAddress = pMacAddress;
    }
}
