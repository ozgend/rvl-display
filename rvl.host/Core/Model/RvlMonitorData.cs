using Rvl.Display.Core.Interfaces;

namespace Rvl.Display.Core.Model;

public sealed class RvlMonitorData : IRvlDevicePayload<RvlMonitorData>
{
    public byte Type => Constants.Report.PayloadType.Data;
    public string CpuName { get; set; }
    public byte CpuUtilization { get; set; }
    public byte CpuTemperature { get; set; }
    public int CpuFan { get; set; }
    public string GpuName { get; set; }
    public byte GpuUtilization { get; set; }
    public byte GpuTemperature { get; set; }
    public int GpuFan { get; set; }
    public int ChasisFan { get; set; }

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
        ChasisFan = 0;
    }

    public byte[] ToReport()
    {
        var report = new byte[Constants.Report.Length];
        report[Constants.Report.PayloadType.ReportIdIndex] = Constants.Report.Null;
        report[Constants.Report.PayloadType.TypeIndex] = Type;
        report[Constants.Report.ValuePayloadIndex.CpuTemp] = CpuTemperature;
        report[Constants.Report.ValuePayloadIndex.CpuUtilization] = CpuUtilization;
        report[Constants.Report.ValuePayloadIndex.CpuFan] = (byte)(CpuFan & 0xFF);
        report[Constants.Report.ValuePayloadIndex.CpuFan + 1] = (byte)((CpuFan >> 8) & 0xFF);
        report[Constants.Report.ValuePayloadIndex.GpuTemp] = GpuTemperature;
        report[Constants.Report.ValuePayloadIndex.GpuUtilization] = GpuUtilization;
        report[Constants.Report.ValuePayloadIndex.GpuFan] = (byte)(GpuFan & 0xFF);
        report[Constants.Report.ValuePayloadIndex.GpuFan + 1] = (byte)((GpuFan >> 8) & 0xFF);
        report[Constants.Report.ValuePayloadIndex.ChasisFan] = (byte)(ChasisFan & 0xFF);
        report[Constants.Report.ValuePayloadIndex.ChasisFan + 1] = (byte)((ChasisFan >> 8) & 0xFF);
        return report;
    }

    public static RvlMonitorData Zero() => new();
}

