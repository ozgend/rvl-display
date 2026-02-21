using HidSharp;
using Microsoft.Extensions.Logging;
using Rvl.Host.App.Core.Interfaces;
using Rvl.Host.App.Core.Model;

namespace Rvl.Host.App.Core.Hardware;

public interface IRvlDevice
{
    bool IsConnected { get; }
    bool Initialize(Action<string> handler);
    Task SendAsync<T>(IRvlDevicePayload<T> payload, CancellationToken ct);
    Task SendRawAsync(byte[] report, CancellationToken ct);
    string GetDeviceInfo();
    void ForceDisconnect();
}

public class RvlDevice(ILogger<RvlDevice> logger) : IRvlDevice, IDisposable
{
    private HidDevice? _device;
    private HidStream? _stream;
    private Action<string>? _eventHandler;
    private bool _isDeviceConnected = false;
    private int _deviceNotFoundEventCount;
    private readonly ILogger<RvlDevice> _logger = logger;

    public bool IsConnected
    {
        get => _isDeviceConnected;
    }

    public bool Initialize(Action<string>? handler)
    {
        _eventHandler = handler;
        DeviceList.Local.Changed += (sender, e) => ConnectToDevice();
        ConnectToDevice();
        return true;
    }

    public async Task SendAsync<T>(IRvlDevicePayload<T> payload, CancellationToken ct)
    {
        try
        {
            if (!_isDeviceConnected || _stream == null)
            {
                _logger?.LogError("Error: Cannot communicate with device, will not send payload");
                return;
            }
            RvlMonitorData? monitorData = null;
            RvlCommandData? commandData = null;

            byte[] report = new byte[Constants.Report.Length];
            report[Constants.Report.Index.ReportId] = Constants.Report.Null;
            report[Constants.Report.Index.Type] = payload.Type;

            switch (payload.Type)
            {
                case Constants.Report.Type.Data:
                    monitorData = payload as RvlMonitorData ?? throw new InvalidCastException("Invalid payload type for MonitorData");
                    report[Constants.Report.Index.CpuTemp] = (byte)monitorData.CpuTemperature;
                    report[Constants.Report.Index.CpuUtilization] = (byte)monitorData.CpuUtilization;
                    report[Constants.Report.Index.GpuTemp] = (byte)monitorData.GpuTemperature;
                    report[Constants.Report.Index.GpuUtilization] = (byte)monitorData.GpuUtilization;
                    break;
                case Constants.Report.Type.Command:
                    commandData = payload as RvlCommandData ?? throw new InvalidCastException("Invalid payload type for CommandData");
                    report[Constants.Report.Index.CommandName] = commandData.Command;
                    report[Constants.Report.Index.CommandValue] = commandData.Value;
                    break;
                default:
                    _logger?.LogError("Error: Unknown payload type, cannot send.");
                    return;
            }

            await SendRawAsync(report, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending data to device.");
        }
    }

    public async Task SendRawAsync(byte[] report, CancellationToken ct)
    {
        try
        {
            if (report.Length != Constants.Report.Length)
            {
                _logger?.LogError($"Error: Report must be {Constants.Report.Length} bytes, received: {report.Length} bytes.");
            }

            if (!_isDeviceConnected || _stream == null)
            {
                return;
            }

            await _stream.WriteAsync(report, ct);

            if (report[Constants.Report.Index.Type] == Constants.Report.Type.Command && report[Constants.Report.Index.CommandName] == Constants.Report.Command.EnterBootloader)
            {
                ForceDisconnect();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending raw report to device.");
        }
    }

    public string GetDeviceInfo()
    {
        return _isDeviceConnected && _device != null
            ? $"RvlDevice: {_device.GetManufacturer()}:{_device.GetProductName()} SN: {_device.GetSerialNumber()} ({_device.VendorID:X4}:{_device.ProductID:X4})"
            : "RvlDevice not connected.";
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _stream = null;
        _device = null;
        GC.SuppressFinalize(this);
    }

    public void ForceDisconnect()
    {
        _isDeviceConnected = false;
        _eventHandler?.Invoke(Constants.Events.DeviceDisconnected);
        _logger?.LogWarning("RvlDevice disconnected.");
    }

    private void ConnectToDevice()
    {
        _device = DeviceList.Local.GetHidDeviceOrNull(Constants.DeviceInfo.VendorId, Constants.DeviceInfo.ProductId);
        _isDeviceConnected = _device != null && _device.TryOpen(out _stream) && _stream != null;

        if (_isDeviceConnected)
        {
            _deviceNotFoundEventCount = 0;
            _eventHandler?.Invoke(Constants.Events.DeviceConnected);
            _logger?.LogInformation($"RvlDevice: {GetDeviceInfo()}");
            _logger?.LogInformation("RvlDevice connected.");
        }
        else
        {
            _deviceNotFoundEventCount++;
            if (_deviceNotFoundEventCount == 1)
            {
                _eventHandler?.Invoke(Constants.Events.DeviceNotFound);
                _logger?.LogWarning("RvlDevice not found.");
            }
        }
    }
}