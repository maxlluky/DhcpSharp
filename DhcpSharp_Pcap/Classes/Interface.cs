
using PcapDotNet.Core;

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
}
