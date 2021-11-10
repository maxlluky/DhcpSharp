using SharpPcap;

class Interface
{
    private static CaptureDeviceList deviceList = CaptureDeviceList.Instance;
    private static ILiveDevice liveDevice = null;

    /// <summary>
    /// Returns all useable Networkinterfaces
    /// </summary>
    /// <returns></returns>
    public CaptureDeviceList getUseableInterfaces()
    {
        return deviceList;
    }

    /// <summary>
    /// Returns the liveDevice used for the Service
    /// </summary>
    /// <returns></returns>
    public ILiveDevice getActiveInterface()
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
