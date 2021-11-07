using System.Collections.Generic;
using System.Net;

public class Subnet
{
    public string vlanID;
    public string domainName;
    public string gatewayIp;
    public string rangeStartIp;
    public string rangeEndIp;

    private List<IPAddress> freeAddressList = new List<IPAddress>();
    private List<IPAddress> leasedAddressList = new List<IPAddress>();

    public void calculateAddresses()
    {
        IPAddress tempAddress = IPAddress.Parse(rangeStartIp);

        while (!tempAddress.Equals(IPAddress.Parse(rangeEndIp)))
        {
            byte[] lastIPBytes = tempAddress.GetAddressBytes();

            if (lastIPBytes[3] == 255)
            {
                lastIPBytes[2] += 1;
                lastIPBytes[3] = 1;
            }
            else
            {
                lastIPBytes[3]++;
            }

            if (lastIPBytes[2] == 255)
            {
                lastIPBytes[1] += 1;
                lastIPBytes[2] = 1;
            }

            if (lastIPBytes[1] == 255)
            {
                lastIPBytes[0] += 1;
                lastIPBytes[1] = 1;
            }

            tempAddress = IPAddress.Parse(lastIPBytes[0] + "." + lastIPBytes[1] + "." + lastIPBytes[2] + "." + lastIPBytes[3]);

            if (!tempAddress.Equals(IPAddress.Parse(rangeEndIp)))
            {
                freeAddressList.Add(tempAddress);
            }
        }
    }

    /// <summary>
    /// Calculates a new free IP address from the specified pool. If no address is free, a null value is returned.
    /// </summary>
    /// <returns></returns>
    public IPAddress getFreeIPAddress()
    {
        if (leasedAddressList.Count != freeAddressList.Count & freeAddressList.Count != 0)
        {
            IPAddress tempAddress = freeAddressList[0];
            freeAddressList.Remove(tempAddress);

            return tempAddress;
        }
        else
        {
            return null;
        }
    }
}
