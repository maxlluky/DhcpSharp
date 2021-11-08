using System.Net;
using System.Net.NetworkInformation;

class Client
{
    public string hostname { get; set; }
    public IPAddress ipaddress { get; set; }
    public uint transactionId { get; set; }
    public PhysicalAddress macAddress { get; set; }

    public Client(string pHostname, IPAddress pIPAddress, uint pTransactionId, PhysicalAddress pMacAddress)
    {
        hostname = pHostname;
        ipaddress = pIPAddress;
        transactionId = pTransactionId;
        macAddress = pMacAddress;
    }
}
