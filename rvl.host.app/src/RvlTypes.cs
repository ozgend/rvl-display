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

    static public RvlMonitorData New(int cpuTemp, int cpuUtil, string cpuName, int gpuTemp, int gpuUtil, string gpuName)
    {
        return new RvlMonitorData
        {
            CpuTemperature = cpuTemp,
            CpuUtilization = cpuUtil,
            CpuName = cpuName,
            GpuTemperature = gpuTemp,
            GpuUtilization = gpuUtil,
            GpuName = gpuName
        };
    }
}

public class RvlCommandData
{
    public byte Command { get; internal set; }
    public byte Value { get; internal set; }

    public RvlCommandData(byte command, byte value = Constants.Report.Null)
    {
        Command = command;
        Value = value;
    }

    static public RvlCommandData New(byte command, byte value = Constants.Report.Null)
    {
        return new RvlCommandData(command, value);
    }
}

public interface IRvlSensorMonitor
{
    bool Initialize();
    RvlMonitorData Poll();
}
