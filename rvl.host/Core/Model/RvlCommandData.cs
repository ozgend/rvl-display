using System.Runtime.InteropServices;
using Rvl.Display.Core.Interfaces;

namespace Rvl.Display.Core.Model;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RvlCommandDataStruct
{
    public byte Command;
    public byte Value;
}

public class RvlCommandData : RvlDevicePayloadBase<RvlCommandDataStruct>
{
    public override byte Type => Constants.Report.Type.Command;
    public override RvlCommandDataStruct Data { get; set; }

    public static RvlCommandData New(byte command, byte? value = 0)
    {
        return new RvlCommandData
        {
            Data = new RvlCommandDataStruct
            {
                Command = command,
                Value = value ?? 0
            }
        };
    }
}

