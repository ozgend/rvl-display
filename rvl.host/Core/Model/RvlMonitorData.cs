using System.Runtime.InteropServices;
using Rvl.Display.Core.Interfaces;

namespace Rvl.Display.Core.Model;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RvlMonitorDataStruct
{
    public byte CpuTemp;
    public byte GpuTemp;
    public byte CpuUtil;
    public byte GpuUtil;
    public ushort CpuFan;
    public ushort GpuFan;
    public ushort ChasisFan;
}

public class RvlMonitorData : RvlDevicePayloadBase<RvlMonitorDataStruct>
{
    public override byte Type => Constants.Report.Type.Data;
    public override RvlMonitorDataStruct Data { get; set; }

    public string CpuName { get; set; } = string.Empty;
    public string GpuName { get; set; } = string.Empty;

    public static RvlMonitorData Empty()
    {
        return new RvlMonitorData
        {
            Data = ZeroStruct(),
        };
    }

    public static RvlMonitorDataStruct ZeroStruct()
    {
        return new RvlMonitorDataStruct
        {
            CpuTemp = 0,
            GpuTemp = 0,
            CpuUtil = 0,
            GpuUtil = 0,
            CpuFan = 0,
            GpuFan = 0,
            ChasisFan = 0
        };
    }
}
