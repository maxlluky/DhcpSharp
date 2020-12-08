using System;
using System.Collections.Generic;
using System.Net;

class AddressPool
{
    //--Gateway 
    private IPAddress gatewyIpAddress;

    //--Scope
    private IPAddress startIPAddress;
    private IPAddress lastIPAddress;
    private long freeAddressesCount;

    //--Leased Addresses
    List<IPAddress> leasedIPAddresses = new List<IPAddress>();

    public IPAddress getGatewayIpAddress()
    {
        return gatewyIpAddress;
    }

    public void setGatewayIpAddress(IPAddress pGatewayIpAddress)
    {
        gatewyIpAddress = pGatewayIpAddress;
    }

    /// <summary>
    /// Defines a new address pool. The start and end address is specified. IP addresses can later be claimed from this range.
    /// </summary>
    /// <param name="pStartIPAddress">Start IP address e.g. 192.168.178.100</param>
    /// <param name="pEndIPAddress">End IP address e.g. 192.168.178.200</param>
    public void setAddressScope(IPAddress pStartIPAddress, IPAddress pEndIPAddress)
    {
        startIPAddress = pStartIPAddress;

        freeAddressesCount = Convert.ToInt64(pEndIPAddress.ToString().Replace(".", string.Empty)) - Convert.ToInt64(pStartIPAddress.ToString().Replace(".", string.Empty));
    }

    /// <summary>
    /// Calculates a new free IP address from the specified pool. If no address is free, a null value is returned.
    /// </summary>
    /// <returns></returns>
    public IPAddress getFreeIPAddress()
    {
        if (true)
        {
            if (leasedIPAddresses.Count != freeAddressesCount)
            {
                lastIPAddress = startIPAddress;
            }

            byte[] lastIPBytes = lastIPAddress.GetAddressBytes();

            lastIPBytes[3]++;


            if (lastIPBytes[3] == 255)
            {
                lastIPBytes[2] += 1;
                lastIPBytes[3] = 1;
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

            lastIPAddress = IPAddress.Parse(lastIPBytes[0] + "." + lastIPBytes[1] + "." + lastIPBytes[2] + "." + lastIPBytes[3]);
            leasedIPAddresses.Add(lastIPAddress);

            return lastIPAddress;
        }
        else
        {
            return null;
        }
    }
}
