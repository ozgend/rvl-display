namespace Rvl.Host.App;

public interface IRvlSensorMonitor
{
    bool Initialize();
    RvlMonitorData Poll();
}

public interface IRvlDevicePayload<T>
{
    byte Type { get; }
}

public interface IRvlHostCliInput
{
    ConsoleModifiers Modifiers { get; }
    ConsoleKey Key { get; }
    RvlCommandData Command { get; }
}

public class RvlMonitorData : IRvlDevicePayload<RvlMonitorData>
{
    public byte Type => Constants.Report.Type.Data;

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

public class RvlCommandData(byte command, byte? value = 0) : IRvlDevicePayload<RvlCommandData>
{
    public byte Type => Constants.Report.Type.Command;

    public byte Command { get; internal set; } = command;
    public byte Value { get; internal set; } = value ?? 0;
}

