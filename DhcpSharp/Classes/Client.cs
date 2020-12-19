using PcapDotNet.Packets.Ethernet;
using System.Net;
class Client
{
    private string hostname { get; set; }
    private IPAddress ipaddress { get; set; }
    private byte[] transactionId { get; set; }
    private MacAddress macAddress { get; set; }

    public Client(string pHostname, IPAddress pIPAddress, byte[] pTransactionId, MacAddress pMacAddress)
    {
        hostname = pHostname;
        ipaddress = pIPAddress;
        transactionId = pTransactionId;
        macAddress = pMacAddress;
    }

    //--GET
    public string getHostname()
    {
        return hostname;
    }

    public IPAddress getIPAddress()
    {
        return ipaddress;
    }

    public byte[] getTransactionId()
    {
        return transactionId;
    }

    public MacAddress getMacAddress()
    {
        return macAddress;
    }

    //--SET
    public void setHostname(string pHostname)
    {
        hostname = pHostname;
    }

    public void setIPAddress(IPAddress pIPAddress)
    {
        ipaddress = pIPAddress;
    }

    public void setTransactionId(byte[] pTransactionId)
    {
        transactionId = pTransactionId;
    }

    public void setMacAddress(MacAddress pMacAddress)
    {
        macAddress = pMacAddress;
    }
}
