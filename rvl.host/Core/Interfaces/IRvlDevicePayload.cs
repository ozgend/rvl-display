using System.Runtime.InteropServices;

namespace Rvl.Display.Core.Interfaces;

public interface IRvlDevicePayload<TStruct> where TStruct : struct
{
    byte Type { get; }
    TStruct Data { get; set; }
    byte[] ToReport();
}

public abstract class RvlDevicePayloadBase<TStruct> : IRvlDevicePayload<TStruct> where TStruct : struct
{
    public abstract byte Type { get; }
    public abstract TStruct Data { get; set; }
    public byte[] ToReport()
    {
        var report = new byte[Constants.Report.Length];
        report[Constants.Report.Index.Type] = Type;
        // dump struct after type +1 offset
        MemoryMarshal.Write(report.AsSpan(Constants.Report.Index.Type + 1), this.Data);
        return report;
    }
}