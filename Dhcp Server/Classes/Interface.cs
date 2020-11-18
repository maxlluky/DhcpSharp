using PcapDotNet.Core;
using System.Diagnostics;
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
    /// Returns the Hw-Address of the active local Networkinterface used by the dhcp server
    /// </summary>
    /// <returns></returns>
    public string getHwAddress()
    {
        //--Get all Networkinterfaces
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        //--Get active PacketDevice
        PacketDevice device = localhost.getActiveInterface();        

        //--Create Empty sting for HwAddress
        string hwAddress = null;

        foreach (NetworkInterface item in networkInterfaces)
        {
            if (item.Id == device.Name.Split('_')[1])
            {
                hwAddress = item.GetPhysicalAddress().ToString();

                for (int i = 2; i <= 16; i += 3)
                {
                    hwAddress = hwAddress.Insert(i, ":");
                }
            }
        }

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
