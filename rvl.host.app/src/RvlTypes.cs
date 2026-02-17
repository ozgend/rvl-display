namespace Rvl.Host.App;

public class RvlMonitorData
{
    public int CpuTemperature { get; internal set; }
    public int CpuUtilization { get; internal set; }
    public string CpuName { get; internal set; }
    public int GpuTemperature { get; internal set; }
    public int GpuUtilization { get; internal set; }
    public string GpuName { get; internal set; }


    public RvlMonitorData()
    {
        CpuTemperature = -1;
        CpuUtilization = -1;
        CpuName = "Unknown";
        GpuTemperature = -1;
        GpuUtilization = -1;
        GpuName = "Unknown";
    }
}

public interface IRvlSensorMonitor
{
    bool Initialize();
    RvlMonitorData Poll();
}
