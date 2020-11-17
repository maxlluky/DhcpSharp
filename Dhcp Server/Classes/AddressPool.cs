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
    private IPAddress endIPAddress;
    private IPAddress lastIPAddress;
    private int availableAddresses;

    //--Leased Addresses
    List<IPAddress> leasedIPAddresses = new List<IPAddress>();

    public void setAddressScope(IPAddress pStartIPAddress, IPAddress pEndIPAddress)
    {
        startIPAddress = pStartIPAddress;
        endIPAddress = pEndIPAddress;

        //--Calculate availableAddresses
        availableAddresses = Convert.ToInt32(pEndIPAddress.ToString().Replace(".", string.Empty)) - Convert.ToInt32(pStartIPAddress.ToString().Replace(".", string.Empty));

        Console.WriteLine(ipAddrToBinary(startIPAddress.ToString()));
    }

    public IPAddress getFreeIPAddress()
    {
        if(lastIPAddress == null)
        {
            lastIPAddress = startIPAddress;
        }

        byte[] lastIPBytes = lastIPAddress.GetAddressBytes();

        lastIPBytes[3]++;


        if (lastIPBytes[3] == 254)
        {
            lastIPBytes[2] += 1;
            lastIPBytes[3] = 1;
        }

        if (lastIPBytes[2] == 254)
        {
            lastIPBytes[1] += 1;
            lastIPBytes[2] = 1;
        }

        if (lastIPBytes[1] == 254)
        {
            lastIPBytes[0] += 1;
            lastIPBytes[1] = 1;
        }

        lastIPAddress = IPAddress.Parse(lastIPBytes[0] + "." + lastIPBytes[1] + "." + lastIPBytes[2] + "." + lastIPBytes[3]);

        if (!endIPAddress.Equals(lastIPAddress))
        {
            return lastIPAddress;
        }

        return null;
    }

    private string ipAddrToBinary(string input)
    {
        // assumes a valid IP Address format
        return String.Join(".", (input.Split('.').Select(x => Convert.ToString(Int32.Parse(x), 2).PadLeft(8, '0'))).ToArray());
    }
}
