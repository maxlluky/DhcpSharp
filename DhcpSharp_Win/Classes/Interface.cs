using PcapDotNet.Core;
using PcapDotNet.Packets.Ethernet;
using System.Collections.Generic;
using System.Net.NetworkInformation;

class Interface
{
    private static IList<LivePacketDevice> deviceList = LivePacketDevice.AllLocalMachine;
    private static PacketDevice liveDevice = null;

    /// <summary>
    /// Returns all useable Networkinterfaces
    /// </summary>
    /// <returns></returns>
    public IList<LivePacketDevice> getUseableInterfaces()
    {
        return deviceList;
    }

    /// <summary>
    /// Returns the liveDevice used for the Service
    /// </summary>
    /// <returns></returns>
    public PacketDevice getActiveInterface()
    {
        return liveDevice;
    }

    /// <summary>
    /// Sets the liveDevice used for the Service
    /// </summary>
    /// <param name="pNr"></param>
    public void setActiveInterface(int pNr)
    {
        liveDevice = deviceList[pNr];
    }

    /// <summary>
    /// Returns the Hw-Address of the active local Networkinterface used by the dhcp server
    /// </summary>
    /// <returns></returns>
    public MacAddress getMacAddress()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        string macAddress = null;

        foreach (NetworkInterface item in networkInterfaces)
        {
            if (item.Id == liveDevice.Name.Split('_')[1])
            {
                macAddress = item.GetPhysicalAddress().ToString();

                for (int i = 2; i <= 16; i += 3)
                {
                    macAddress = macAddress.Insert(i, ":");
                }
            }
        }

        return new MacAddress(macAddress);
    }
}
