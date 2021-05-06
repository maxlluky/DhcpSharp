using PcapDotNet.Core;
using System.Collections.Generic;

class Localhost
{
    private static IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
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
