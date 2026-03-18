using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;
using Terminal.Gui;

namespace Rvl.Display.Cli;

public class RvlDeviceCommand
{
    public required string Name { get; init; }
    public required IRvlDevicePayload<RvlCommandDataStruct> Payload { get; init; }
}

public class GuiCommand : RvlDeviceCommand
{
    public required int Category { get; init; }
    public required int Order { get; init; }
    public required Key HotKey { get; init; }
    // public string HotKeyName => $"{(HotKey.ToStringModifiers != ConsoleModifiers.None ? $"{Modifiers} + " : "")}{Key}";
}

public enum CommandCategory
{
    Messages = 1,
    Brightness = 2,
    Device = 3
}

public static class Commands
{
    public static readonly List<GuiCommand> List =
    [
        new GuiCommand { Name = "MessageClear", Category = (int)CommandCategory.Messages, Order = 1, HotKey = Key.D1, Payload = RvlCommandData.New(Constants.Report.Command.MessageClear) },
        new GuiCommand { Name = "MessageAwait", Category = (int)CommandCategory.Messages, Order = 2, HotKey = Key.D2, Payload = RvlCommandData.New(Constants.Report.Command.MessageAwait) },
        new GuiCommand { Name = "MessageCheckHost", Category = (int)CommandCategory.Messages, Order = 3, HotKey = Key.D3, Payload = RvlCommandData.New(Constants.Report.Command.MessageCheckHost) },
        new GuiCommand { Name = "MessageHighTemp", Category = (int)CommandCategory.Messages, Order = 4, HotKey = Key.D4, Payload = RvlCommandData.New(Constants.Report.Command.MessageHighTemp) },
        new GuiCommand { Name = "MessageLowRpm", Category = (int)CommandCategory.Messages, Order = 5, HotKey = Key.D5, Payload = RvlCommandData.New(Constants.Report.Command.MessageLowRpm) },
        new GuiCommand { Name = "SetBrightnessHigh", Category = (int)CommandCategory.Brightness, Order = 1, HotKey = Key.B, Payload = RvlCommandData.New(Constants.Report.Command.SetBrightnessHigh) },
        new GuiCommand { Name = "SetBrightnessMedium", Category = (int)CommandCategory.Brightness, Order = 2, HotKey = Key.N, Payload = RvlCommandData.New(Constants.Report.Command.SetBrightnessMedium) },
        new GuiCommand { Name = "SetBrightnessLow", Category = (int)CommandCategory.Brightness, Order = 3, HotKey = Key.M, Payload = RvlCommandData.New(Constants.Report.Command.SetBrightnessLow) },
        new GuiCommand { Name = "LedOn", Category = (int)CommandCategory.Brightness, Order = 4, HotKey = Key.O, Payload = RvlCommandData.New(Constants.Report.Command.LedOn) },
        new GuiCommand { Name = "LedOff", Category = (int)CommandCategory.Brightness, Order = 5, HotKey = Key.P, Payload = RvlCommandData.New(Constants.Report.Command.LedOff) },
        new GuiCommand { Name = "ClearDisplay", Category = (int)CommandCategory.Device, Order = 1, HotKey = Key.C.WithAlt, Payload = RvlCommandData.New(Constants.Report.Command.ClearDisplay) },
        new GuiCommand { Name = "RestartDevice", Category = (int)CommandCategory.Device, Order = 2, HotKey = Key.R.WithAlt, Payload = RvlCommandData.New(Constants.Report.Command.RestartDevice) },
        new GuiCommand { Name = "EnterBootloader", Category = (int)CommandCategory.Device, Order = 3, HotKey = Key.F.WithAlt, Payload = RvlCommandData.New(Constants.Report.Command.EnterBootloader) },
    ];
}