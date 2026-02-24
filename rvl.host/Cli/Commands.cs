using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;
using Terminal.Gui;

namespace Rvl.Display.Cli;

public class RvlDeviceCommand
{
    public required string Name { get; init; }
    public required IRvlDevicePayload<RvlCommandData> Payload { get; init; }
}

public class GuiCommand : RvlDeviceCommand
{
    public required int Category { get; init; }
    public required int Order { get; init; }
    public required Key HotKey { get; init; }
    // public string HotKeyName => $"{(HotKey.ToStringModifiers != ConsoleModifiers.None ? $"{Modifiers} + " : "")}{Key}";
}

public static class Commands
{
    public static readonly List<GuiCommand> List =
    [
        new GuiCommand { Name = "MessageClear", Category = 1, Order = 1, HotKey = Key.D1, Payload = RvlCommandData.New(Constants.Report.Command.MessageClear) },
        new GuiCommand { Name = "MessageAwait", Category = 1, Order = 2, HotKey = Key.D2, Payload = RvlCommandData.New(Constants.Report.Command.MessageAwait) },
        new GuiCommand { Name = "MessageCheckHost", Category = 1, Order = 3, HotKey = Key.D3, Payload = RvlCommandData.New(Constants.Report.Command.MessageCheckHost) },
        new GuiCommand { Name = "MessageHighTemp", Category = 1, Order = 4, HotKey = Key.D4, Payload = RvlCommandData.New(Constants.Report.Command.MessageHighTemp) },
        new GuiCommand { Name = "MessageLowRpm", Category = 1, Order = 5, HotKey = Key.D5, Payload = RvlCommandData.New(Constants.Report.Command.MessageLowRpm) },
        new GuiCommand { Name = "SetBrightnessHigh", Category = 2, Order = 1, HotKey = Key.B, Payload = RvlCommandData.New(Constants.Report.Command.SetBrightnessHigh) },
        new GuiCommand { Name = "SetBrightnessMedium", Category = 2, Order = 2, HotKey = Key.N, Payload = RvlCommandData.New(Constants.Report.Command.SetBrightnessMedium) },
        new GuiCommand { Name = "SetBrightnessLow", Category = 2, Order = 3, HotKey = Key.M, Payload = RvlCommandData.New(Constants.Report.Command.SetBrightnessLow) },
        new GuiCommand { Name = "LedOn", Category = 2, Order = 4, HotKey = Key.O, Payload = RvlCommandData.New(Constants.Report.Command.LedOn) },
        new GuiCommand { Name = "LedOff", Category = 2, Order = 5, HotKey = Key.P, Payload = RvlCommandData.New(Constants.Report.Command.LedOff) },
        new GuiCommand { Name = "ClearDisplay", Category = 3, Order = 1, HotKey = Key.C.WithAlt, Payload = RvlCommandData.New(Constants.Report.Command.ClearDisplay) },
        new GuiCommand { Name = "RestartDevice", Category = 3, Order = 2, HotKey = Key.R.WithAlt, Payload = RvlCommandData.New(Constants.Report.Command.RestartDevice) },
        new GuiCommand { Name = "EnterBootloader", Category = 3, Order = 3, HotKey = Key.F.WithAlt, Payload = RvlCommandData.New(Constants.Report.Command.EnterBootloader) },
    ];
}