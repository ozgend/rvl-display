using Rvl.Host.App.Core.Interfaces;

namespace Rvl.Host.App.Core.Model;

public class RvlMonitorData : IRvlDevicePayload<RvlMonitorData>
{
    public byte Type => Constants.Report.Type.Data;
    public string CpuName { get; internal set; }
    public int CpuTemperature { get; internal set; }
    public int CpuUtilization { get; internal set; }
    public int CpuFan { get; internal set; }
    public string GpuName { get; internal set; }
    public int GpuTemperature { get; internal set; }
    public int GpuUtilization { get; internal set; }
    public int GpuFan { get; internal set; }

    public RvlMonitorData()
    {
        CpuName = "Unknown";
        CpuTemperature = -1;
        CpuUtilization = -1;
        CpuFan = -1;
        GpuName = "Unknown";
        GpuTemperature = -1;
        GpuUtilization = -1;
        GpuFan = -1;
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
}

