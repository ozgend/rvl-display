using HidSharp;

namespace Rvl.Host.App;

public interface IRvlDevice
{
    bool IsConnected { get; }
    bool Initialize(Action<string> handler);
    Task SendData(RvlMonitorData data);
    Task SendCommand(byte command, byte value = Constants.Report.Null);
    string GetDeviceInfo();
    void ForceDisconnect();
}

public class RvlDevice : IRvlDevice, IDisposable
{
    private HidDevice? _device;
    private HidStream? _stream;
    private Action<string>? _eventHandler;
    private bool _isDeviceConnected = false;
    private int _deviceNotFoundEventCount;

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

    public async Task SendData(RvlMonitorData data)
    {
        if (!_isDeviceConnected || _stream == null)
        {
            Console.WriteLine("Error: Cannot send data, no device connection.");
            return;
        }

        byte[] report = new byte[Constants.Report.Length];
        report[Constants.Report.Index.ReportId] = Constants.Report.Null;
        report[Constants.Report.Index.Type] = Constants.Report.DataPayload;
        report[Constants.Report.Index.CpuTemp] = (byte)data.CpuTemperature;
        report[Constants.Report.Index.CpuUtilization] = (byte)data.CpuUtilization;
        report[Constants.Report.Index.GpuTemp] = (byte)data.GpuTemperature;
        report[Constants.Report.Index.GpuUtilization] = (byte)data.GpuUtilization;

        await _stream.WriteAsync(report);
    }

    public async Task SendCommand(byte command, byte value = Constants.Report.Null)
    {
        if (!_isDeviceConnected || _stream == null)
        {
            Console.WriteLine("Error: Cannot send command, no device connection.");
            return;
        }

        byte[] report = new byte[Constants.Report.Length];
        report[Constants.Report.Index.ReportId] = Constants.Report.Null;
        report[Constants.Report.Index.Type] = Constants.Report.CommandPayload;
        report[Constants.Report.Index.CommandName] = command;
        report[Constants.Report.Index.CommandValue] = value;

        await _stream.WriteAsync(report);
    }

    public string GetDeviceInfo()
    {
        return _isDeviceConnected && _device != null ? $"RvlDevice: {_device.GetManufacturer()}:{_device.GetProductName()} SN: {_device.GetSerialNumber()} ({_device.VendorID:X4}:{_device.ProductID:X4})" : "RvlDevice not connected.";
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
        Console.WriteLine("RvlDevice disconnected.");
    }

    private void ConnectToDevice()
    {
        _device = DeviceList.Local.GetHidDeviceOrNull(Constants.DeviceInfo.VendorId, Constants.DeviceInfo.ProductId);
        _isDeviceConnected = _device != null && _device.TryOpen(out _stream) && _stream != null;

        if (_isDeviceConnected)
        {
            _deviceNotFoundEventCount = 0;
            _eventHandler?.Invoke(Constants.Events.DeviceConnected);
            Console.WriteLine($"RvlDevice: {GetDeviceInfo()}");
            Console.WriteLine("RvlDevice connected.");
        }
        else
        {
            _deviceNotFoundEventCount++;
            if (_deviceNotFoundEventCount == 1)
            {
                _eventHandler?.Invoke(Constants.Events.DeviceNotFound);
                Console.WriteLine("RvlDevice not found.");
            }
        }
    }
}