using Rvl.Host.App.Core.Interfaces;

namespace Rvl.Host.App.Core.Model;

public class RvlCommandData(byte command, byte? value = 0) : IRvlDevicePayload<RvlCommandData>
{
    public byte Type => Constants.Report.Type.Command;
    public byte Command { get; internal set; } = command;
    public byte Value { get; internal set; } = value ?? 0;

    public byte[] ToReport()
    {
        var report = new byte[Constants.Report.Length];
        report[Constants.Report.Index.ReportId] = Constants.Report.Null;
        report[Constants.Report.Index.Type] = Constants.Report.Type.Command;
        report[Constants.Report.Index.CommandName] = Command;
        report[Constants.Report.Index.CommandValue] = Value;
        return report;
    }
}

