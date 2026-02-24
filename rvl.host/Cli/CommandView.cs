using System.IO.Pipes;
using CommunityToolkit.Mvvm.Messaging;
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

    public CommandView()
    {
        InitializeComponent();
        _pipeClient = new NamedPipeClientStream(".", Constants.PipeName, PipeDirection.InOut);
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
        if (((Button)sender)?.Data is not RvlDeviceCommand command)
        {
            return;
        }

        SendCommandAsync(command.Payload, command.Name).ConfigureAwait(false);
    }


    private async Task SendCommandAsync(IRvlDevicePayload<RvlCommandData> payload, string commandName)
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