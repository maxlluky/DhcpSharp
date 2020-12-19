using PcapDotNet.Packets.Ethernet;
using System.Net;
class Client
{
    public string hostname { get; set; }
    public IPAddress ipAddress { get; set; }
    public byte[] transactionId { get; set; }
    public MacAddress macAddress { get; set; }
    public string status { get; set; }

    public Client(string pHostname, IPAddress pIPAddress, byte[] pTransactionId, MacAddress pMacAddress, string pStatus)
    {
        hostname = pHostname;
        ipAddress = pIPAddress;
        transactionId = pTransactionId;
        macAddress = pMacAddress;
        status = pStatus;
    }
}
