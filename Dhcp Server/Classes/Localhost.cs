using PcapDotNet.Core;
using System.Collections.Generic;

class Localhost
{
    //--Retrieve the device list from the local machine
    private static IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

    //--Represents the active interface to use for the dhcp service
    private static PacketDevice packetDevice;

    public IList<LivePacketDevice> getUseableInterfaces()
    {
        return allDevices;
    }

    public PacketDevice getActiveInterface()
    {
        return packetDevice;
    }

    public void setActiveInterface(int pNr)
    {
        packetDevice = allDevices[pNr];
    }
}