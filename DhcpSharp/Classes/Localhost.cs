using SharpPcap;

class Localhost
{
    private static CaptureDeviceList deviceList = CaptureDeviceList.Instance;
    private static ILiveDevice liveDevice;

    public CaptureDeviceList getUseableInterfaces()
    {
        return deviceList;
    }

    public ILiveDevice getActiveInterface()
    {
        return liveDevice;
    }

    public void setActiveInterface(int pNr)
    {
        liveDevice = deviceList[pNr];
    }
}
