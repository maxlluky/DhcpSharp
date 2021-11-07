using System.Net;
using System.Net.NetworkInformation;

class Client
{
    public string hostname { get; set; }
    public IPAddress ipaddress { get; set; }
    public byte[] transactionId { get; set; }
    public PhysicalAddress macAddress { get; set; }

    public Client(string pHostname, IPAddress pIPAddress, byte[] pTransactionId, PhysicalAddress pMacAddress)
    {
        hostname = pHostname;
        ipaddress = pIPAddress;
        transactionId = pTransactionId;
        macAddress = pMacAddress;
    }
}
