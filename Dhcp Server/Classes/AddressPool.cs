using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

class AddressPool
{
    //--Scope
    private IPAddress startIPAddress;
    private IPAddress lastIPAddress;
    private int availableAddresses;

    //--Leased Addresses
    List<IPAddress> leasedIPAddresses = new List<IPAddress>();

    /// <summary>
    /// Returns the current number of available addresses
    /// </summary>
    /// <returns></returns>
    public int getFreeAddressCount()
    {
        return availableAddresses;
    }

    /// <summary>
    /// Defines a new address pool. The start and end address is specified. IP addresses can later be claimed from this range.
    /// </summary>
    /// <param name="pStartIPAddress">Start IP address e.g. 192.168.178.100</param>
    /// <param name="pEndIPAddress">End IP address e.g. 192.168.178.200</param>
    public void setAddressScope(IPAddress pStartIPAddress, IPAddress pEndIPAddress)
    {
        startIPAddress = pStartIPAddress;

        //--Calculate availableAddresses
        availableAddresses = Convert.ToInt32(pEndIPAddress.ToString().Replace(".", string.Empty)) - Convert.ToInt32(pStartIPAddress.ToString().Replace(".", string.Empty));
    }

    /// <summary>
    /// Calculates a new free IP address from the specified pool. If no address is free, a null value is returned.
    /// </summary>
    /// <returns></returns>
    public IPAddress getFreeIPAddress()
    {
        if (leasedIPAddresses.Count != availableAddresses)
        {
            if (lastIPAddress == null)
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
