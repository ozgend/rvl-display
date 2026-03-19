using System.IO.Pipes;
using LibreHardwareMonitor.Hardware;
using Rvl.Display.Core;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;
using Terminal.Gui;

namespace Rvl.Display.Cli;

enum LabelStatus
{
    Info,
    Ok,
    Warn,
    Error
}

internal partial class CommandView
{
    private readonly NamedPipeClientStream _pipeClient;
    private readonly Computer _computer;

    public CommandView()
    {
        InitializeComponent();
        _pipeClient = new NamedPipeClientStream(".", Constants.PipeName, PipeDirection.InOut);
        _computer = new Computer()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsStorageEnabled = true,
            IsBatteryEnabled = false,
            IsNetworkEnabled = false,
            IsPsuEnabled = false
        };
    }

    public void Receive(RvlDeviceCommand message)
    {
        SetMessage(LabelStatus.Info, $"Received Command: {message.Name}");
    }

    public async Task ConnectToPipeAsync()
    {
        if (_pipeClient.IsConnected)
        {
            return;
        }

        try
        {
            await _pipeClient.ConnectAsync(Constants.PipeConnectTimeoutMs);
            SetStatus(LabelStatus.Info, $"Connected @{Constants.PipeAddress}");
        }
        catch (TimeoutException)
        {
            SetStatus(LabelStatus.Error, $"Timeout while connecting @{Constants.PipeAddress}");
        }
        catch (Exception ex)
        {
            SetStatus(LabelStatus.Error, $"Failed to connect @{Constants.PipeAddress}: {ex.Message}");
        }
    }

    private void HandleButtonEvent(object sender, EventArgs e)
    {
        if (((Button)sender)?.Data is not GuiCommand command)
        {
            return;
        }

        if (command.IsLocalCommand)
        {
            SendLocalCommand(command.Payload.ToReport()[Constants.Report.Index.Command], command.Name);
        }
        else
        {
            _ = SendRemoteCommandAsync(command.Payload, command.Name).ConfigureAwait(false);
        }
    }

    private void SendLocalCommand(byte commandByte, string commandName)
    {
        _computer.Open();
        if (_computer.Hardware?.Any() != true)
        {
            SetMessage(LabelStatus.Warn, $"No hardware found for local command: {commandName}");
            return;
        }

        SetMessage(LabelStatus.Info, $"Executing local command: {commandName}");

        // list computer hardware and sensors to ./hw.txt for debugging
        string filepath = AppContext.BaseDirectory + "/hw.txt";

        using StreamWriter hwWriter = new(filepath, new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.Read
        });
        foreach (var hardware in _computer.Hardware)
        {
            hwWriter.WriteLine($"Hardware: {hardware.Name} ({hardware.HardwareType})");
            hardware.Update();
            foreach (var sensor in hardware.Sensors)
            {
                hwWriter.WriteLine($"  Sensor: {sensor.Name} ({sensor.SensorType}) - Value: {sensor.Value}");
            }
        }
        hwWriter.Flush();


        SetMessage(LabelStatus.Ok, $"Executed local command: {commandName} (see hw.txt for details)");
    }

    private async Task SendRemoteCommandAsync(IRvlDevicePayload<RvlCommandDataStruct> payload, string commandName)
    {
        await ConnectToPipeAsync();
        if (!_pipeClient.IsConnected)
        {
            SetMessage(LabelStatus.Error, $"Cannot send {commandName}: Not connected.");
            return;
        }

        try
        {
            var payloadBuffer = payload.ToReport().AsMemory();
            await _pipeClient.WriteAsync(payloadBuffer);
            await _pipeClient.FlushAsync();

            byte[] responseBuffer = new byte[1];
            int bytesRead = await _pipeClient.ReadAsync(responseBuffer.AsMemory());

            if (bytesRead > 0 && responseBuffer[0] == Constants.Report.Ok)
            {
                SetMessage(LabelStatus.Ok, $"Sent [{commandName}]");
            }
            else
            {
                SetMessage(LabelStatus.Warn, $"[{commandName}] sent, but server returned error.");
            }
        }
        catch (Exception ex)
        {
            SetMessage(LabelStatus.Error, $"Failed to send [{commandName}]: {ex.Message}");
        }
    }

    private void SetMessage(LabelStatus status, string text)
    {
        _messageLabel.ColorScheme = status switch
        {
            LabelStatus.Info => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Black) },
            LabelStatus.Ok => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black) },
            LabelStatus.Warn => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black) },
            LabelStatus.Error => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black) },
            _ => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.White, Color.Black) }
        };
        _messageLabel.Text = text;
    }

    private void SetStatus(LabelStatus status, string text)
    {
        _statusLabel.ColorScheme = status switch
        {
            LabelStatus.Info => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Black) },
            LabelStatus.Ok => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black) },
            LabelStatus.Warn => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black) },
            LabelStatus.Error => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black) },
            _ => new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.White, Color.Black) }
        };
        _statusLabel.Text = $"Status: {text}";
    }

}