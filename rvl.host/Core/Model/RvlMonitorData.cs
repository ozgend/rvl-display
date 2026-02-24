using Rvl.Display.Core.Interfaces;

namespace Rvl.Display.Core.Model;

public sealed class RvlMonitorData : IRvlDevicePayload<RvlMonitorData>
{
    public byte Type => Constants.Report.Type.Data;
    public string CpuName { get; set; }
    public int CpuTemperature { get; set; }
    public int CpuUtilization { get; set; }
    public int CpuFan { get; set; }
    public string GpuName { get; set; }
    public int GpuTemperature { get; set; }
    public int GpuUtilization { get; set; }
    public int GpuFan { get; set; }

    public RvlMonitorData()
    {
        CpuName = "Unknown";
        CpuTemperature = 0;
        CpuUtilization = 0;
        CpuFan = 0;
        GpuName = "Unknown";
        GpuTemperature = 0;
        GpuUtilization = 0;
        GpuFan = 0;
    }

    public byte[] ToReport()
    {
        var report = new byte[Constants.Report.Length];
        report[Constants.Report.Index.ReportId] = Constants.Report.Null;
        report[Constants.Report.Index.Type] = Constants.Report.Type.Data;
        report[Constants.Report.Index.CpuTemp] = (byte)CpuTemperature;
        report[Constants.Report.Index.CpuUtilization] = (byte)CpuUtilization;
        report[Constants.Report.Index.GpuTemp] = (byte)GpuTemperature;
        report[Constants.Report.Index.GpuUtilization] = (byte)GpuUtilization;
        report[Constants.Report.Index.CpuFan] = (byte)CpuFan;
        report[Constants.Report.Index.GpuFan] = (byte)GpuFan;
        return report;
    }

    public static RvlMonitorData Zero() => new();
}

