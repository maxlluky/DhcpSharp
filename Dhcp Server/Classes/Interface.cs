using PcapDotNet.Core;
using System.Net;
using System.Net.NetworkInformation;

class Interface
{
    //--Classes
    private Localhost localhost = new Localhost();

    //--InterfaceIndex for HwAddress
    private int interfaceIndex;

    public Interface(Localhost pLocalhost)
    {
        localhost = pLocalhost;
    }

    public int getInterfaceIndex()
    {
        return interfaceIndex;
    }

    public void setInterfaceIndex(int pInterfaceIndex)
    {
        interfaceIndex = pInterfaceIndex;
    }

    /// <summary>
    /// Returns the Hw-Address of a local Networkinterface
    /// </summary>
    /// <returns></returns>
    public string getHwAddress()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        string rawHwAddress = networkInterfaces[interfaceIndex].GetPhysicalAddress().ToString();

        for (int i = 2; i <= 16; i += 3)
        {
            rawHwAddress = rawHwAddress.Insert(i, ":");
        }

        string hwAddress = rawHwAddress;
        return hwAddress;
    }

    public IPAddress getIPAddress()
    {
        IPAddress interfaceAddress = null;

        foreach (DeviceAddress deviceAddress in localhost.getActiveInterface().Addresses)
        {
            interfaceAddress = IPAddress.Parse(deviceAddress.Address.ToString().Replace("Internet ", ""));
        }

        return interfaceAddress;
    }

    public IPAddress getNetmask()
    {
        IPAddress interfaceNetmask = null;

        foreach (DeviceAddress deviceAddress in localhost.getActiveInterface().Addresses)
        {
            interfaceNetmask = IPAddress.Parse(deviceAddress.Netmask.ToString().Replace("Internet ", ""));
        }

        return interfaceNetmask;
    }

    public IPAddress getBroadcast()
    {
        IPAddress interfaceBroadcast = null;

        foreach (DeviceAddress deviceAddress in localhost.getActiveInterface().Addresses)
        {
            interfaceBroadcast = IPAddress.Parse(deviceAddress.Broadcast.ToString().Replace("Internet ", ""));
        }

        return interfaceBroadcast;
    }
}
