using System.Net;
class Client
{
    private string hostname { get; set; }
    private IPAddress ipaddress { get; set; }
    private byte[] transactionId { get; set; }

    public Client(string pHostname, IPAddress pIPAddress, byte[] pTransactionId)
    {
        hostname = pHostname;
        ipaddress = pIPAddress;
        transactionId = pTransactionId;
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
}
