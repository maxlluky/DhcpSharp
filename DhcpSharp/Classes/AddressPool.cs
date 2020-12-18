using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

class AddressPool
{
    //--Gateway 
    private IPAddress gatewyIpAddress;

    //--Domain Name
    private string domainName;

    //--Scope
    private IPAddress startIPAddress;
    private IPAddress endIPAddress;

    //--List with avilabel addresses
    List<IPAddress> freeAddressList = new List<IPAddress>();

    //--List with leased addresses
    List<IPAddress> leasedAddressList = new List<IPAddress>();

    public IPAddress getGatewayIpAddress()
    {
        return gatewyIpAddress;
    }

    public void setGatewayIpAddress(IPAddress pGatewayIpAddress)
    {
        gatewyIpAddress = pGatewayIpAddress;
    }

    public string getDomainName()
    {
        return domainName;
    }

    public void setDomainName(string pDomainName)
    {
        domainName = pDomainName;
    }

    public List<IPAddress> getFreeAddressList()
    {
        return freeAddressList;
    }

    /// <summary>
    /// Defines a new address pool. The start and end address is specified. IP addresses can later be claimed from this range.
    /// </summary>
    /// <param name="pStartIPAddress">Start IP address e.g. 192.168.178.100</param>
    /// <param name="pEndIPAddress">End IP address e.g. 192.168.178.200</param>
    public void setAddressScope(IPAddress pStartIPAddress, IPAddress pEndIPAddress)
    {
        startIPAddress = pStartIPAddress;
        endIPAddress = pEndIPAddress;

        calculateFreeAddressList();
    }

    private void calculateFreeAddressList()
    {
        IPAddress tempAddress = startIPAddress;

        while (!tempAddress.Equals(endIPAddress))
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

            if (!tempAddress.Equals(endIPAddress))
            {
                freeAddressList.Add(tempAddress);
            }
        }

        Debug.WriteLine("The scope contains " + freeAddressList.Count + " free IP-Addresses");
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
