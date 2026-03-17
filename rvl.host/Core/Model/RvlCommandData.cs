using Rvl.Display.Core.Interfaces;

namespace Rvl.Display.Core.Model;

public sealed class RvlCommandData(byte command, byte? value = 0) : IRvlDevicePayload<RvlCommandData>
{
    public byte Type => Constants.Report.PayloadType.Command;
    public byte Command { get; set; } = command;
    public byte Value { get; set; } = value ?? 0;

    public byte[] ToReport()
    {
        var report = new byte[Constants.Report.Length];
        report[Constants.Report.PayloadType.ReportIdIndex] = Constants.Report.Null;
        report[Constants.Report.PayloadType.TypeIndex] = Type;
        report[Constants.Report.CommandPayloadIndex.CommandName] = Command;
        report[Constants.Report.CommandPayloadIndex.CommandValue] = Value;
        return report;
    }

    public static IRvlDevicePayload<RvlCommandData> New(byte command, byte? value = 0)
    {
        return new RvlCommandData(command, value);
    }
}

