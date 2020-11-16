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
        return null;
    }

    private string ipAddrToBinary(string input)
    {
        // assumes a valid IP Address format
        return String.Join(".", (input.Split('.').Select(x => Convert.ToString(Int32.Parse(x), 2).PadLeft(8, '0'))).ToArray());
    }
}
