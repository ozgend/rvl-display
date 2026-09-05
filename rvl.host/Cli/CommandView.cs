using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.ServiceProcess;
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
    private NamedPipeClientStream _pipeClient;
    private readonly Computer _computer;
    private CancellationTokenSource? _streamCancellation;

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

    public async Task<bool> ConnectToPipeAsync()
    {
        if (_pipeClient.IsConnected)
        {
            Application.Invoke(() => SetStatus(LabelStatus.Info, $"Connected @{Constants.PipeAddress}"));
            return true;
        }

        _pipeClient.Dispose();
        _pipeClient = new NamedPipeClientStream(".", Constants.PipeName, PipeDirection.InOut);

        try
        {
            await _pipeClient.ConnectAsync(Constants.PipeConnectTimeoutMs);
            Application.Invoke(() => SetStatus(LabelStatus.Info, $"Connected @{Constants.PipeAddress}"));
            return true;
        }
        catch (System.TimeoutException)
        {
            Application.Invoke(() => SetStatus(LabelStatus.Error, $"Timeout while connecting @{Constants.PipeAddress}"));
        }
        catch (Exception ex)
        {
            Application.Invoke(() => SetStatus(LabelStatus.Error, $"Failed to connect @{Constants.PipeAddress}: {ex.Message}"));
        }

        return false;
    }

    private void HandleButtonEvent(object sender, EventArgs e)
    {
        if (((Button)sender)?.Data is not GuiCommand command)
        {
            return;
        }

        _ = ExecuteCommandAsync(command);
    }

    private async Task ExecuteCommandAsync(GuiCommand command)
    {
        await ConnectToPipeAsync();

        if (command.IsLocalCommand)
        {
            _ = ExecuteLocalCommandAsync(command.Payload.ToReport()[Constants.Report.Index.Command], command.Name);
        }
        else
        {
            _ = SendRemoteCommandAsync(command.Payload, command.Name).ConfigureAwait(false);
        }
    }

    private async Task ExecuteLocalCommandAsync(byte commandByte, string commandName)
    {
        switch (commandByte)
        {
            case Constants.Report.Command.ServiceStart:
            case Constants.Report.Command.ServiceStop:
            case Constants.Report.Command.ServiceRestart:
                await ManageServiceAsync(commandByte, commandName);
                break;
            case Constants.Report.Command.StartStream:
                StartStream();
                break;
            case Constants.Report.Command.StopStream:
                StopStream();
                break;
            default:
                ListSensors(commandName);
                break;
        }
    }

    private void ListSensors(string commandName)
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

    private async Task ManageServiceAsync(byte commandByte, string commandName)
    {
        SetMessage(LabelStatus.Info, $"Executing {commandName}...");
        try
        {
            await Task.Run(() =>
            {
                using var service = new ServiceController(Constants.ServiceName);
                service.Refresh();

                if (commandByte == Constants.Report.Command.ServiceStart)
                {
                    StartService(service);
                }
                else if (commandByte == Constants.Report.Command.ServiceStop)
                {
                    StopService(service);
                }
                else
                {
                    StopService(service);
                    service.Refresh();
                    StartService(service);
                }
            });
            SetMessage(LabelStatus.Ok, $"Executed {commandName}");
        }
        catch (Exception ex)
        {
            SetMessage(LabelStatus.Error, $"Failed to execute {commandName}: {ex.Message}");
        }
    }

    private static void StartService(ServiceController service)
    {
        if (service.Status is ServiceControllerStatus.Running or ServiceControllerStatus.StartPending)
        {
            return;
        }

        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
    }

    private static void StopService(ServiceController service)
    {
        if (service.Status is ServiceControllerStatus.Stopped or ServiceControllerStatus.StopPending)
        {
            if (service.Status == ServiceControllerStatus.StopPending)
            {
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
            }
            return;
        }

        service.Stop();
        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
    }

    private void StartStream()
    {
        if (_streamCancellation is not null)
        {
            SetMessage(LabelStatus.Info, "Sensor stream is already running.");
            return;
        }

        _streamCancellation = new CancellationTokenSource();
        _ = RunStreamAsync(_streamCancellation);
    }

    private void StopStream()
    {
        _streamCancellation?.Cancel();
        _streamCancellation?.Dispose();
        _streamCancellation = null;
        SetMessage(LabelStatus.Ok, "Sensor stream stopped.");
    }

    private async Task RunStreamAsync(CancellationTokenSource cancellation)
    {
        var ct = cancellation.Token;
        try
        {
            await using var stream = new NamedPipeClientStream(".", Constants.StreamPipeName, PipeDirection.In, PipeOptions.Asynchronous);
            await stream.ConnectAsync(Constants.PipeConnectTimeoutMs, ct);
            Application.Invoke(() => SetMessage(LabelStatus.Ok, $"Streaming @{Constants.StreamPipeAddress}"));

            var report = new byte[Constants.Report.Length];
            while (!ct.IsCancellationRequested)
            {
                await stream.ReadExactlyAsync(report, ct);
                if (report[Constants.Report.Index.Type] != Constants.Report.Type.Data)
                {
                    continue;
                }

                var data = MemoryMarshal.Read<RvlMonitorDataStruct>(report.AsSpan(Constants.Report.Index.Type + 1));
                Application.Invoke(() => UpdateStreamTable(data));
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Application.Invoke(() => SetMessage(LabelStatus.Error, $"Sensor stream failed: {ex.Message}"));
        }
        finally
        {
            cancellation.Dispose();
            if (ReferenceEquals(_streamCancellation, cancellation))
            {
                _streamCancellation = null;
            }
        }
    }

    private void UpdateStreamTable(RvlMonitorDataStruct data)
    {
        _streamTable.Rows[0].ItemArray = ["     CPU ", $" {data.CpuUtil,3} % ", $" {data.CpuTemp,3} *C ", $" {data.CpuFan,4} rpm"];
        _streamTable.Rows[1].ItemArray = ["     GPU ", $" {data.GpuUtil,3} % ", $" {data.GpuTemp,3} *C ", $" {data.GpuFan,4} rpm"];
        _streamTable.Rows[2].ItemArray = [" CHASSIS ", " ", $" {data.ChassisTemp,3} *C", $" {data.ChassisFan,4} rpm"];
        _streamTableView.Update();
    }

    private async Task SendRemoteCommandAsync(IRvlDevicePayload<RvlCommandDataStruct> payload, string commandName)
    {
        if (!await ConnectToPipeAsync())
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