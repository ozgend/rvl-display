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
        // windows HID report: 1 byte reportid + 64 byte payload
        var report = new byte[Constants.Report.Length + 1];
        report[0] = Constants.Report.Null;
        report[1] = Type;

        // dump struct to array +2 offset 
        MemoryMarshal.Write(report.AsSpan(2), this.Data);
        return report;
    }
}