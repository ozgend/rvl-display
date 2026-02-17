using HidSharp;

namespace Rvl.Host.App;

public interface IRvlDevice
{
    bool Initialize();
    Task SendData(RvlMonitorData data);
    Task SendCommand(byte command, byte value = Constants.Report.Null);
    string GetDeviceInfo();
}

public class RvlDevice : IRvlDevice, IDisposable
{
    private HidDevice? _device;
    private HidStream? _stream;


    public bool Initialize()
    {
        Console.WriteLine("init: RvlDevice");

        _device = DeviceList.Local.GetHidDeviceOrNull(Constants.DeviceInfo.VendorId, Constants.DeviceInfo.ProductId);

        if (_device == null)
        {
            Console.WriteLine("Error: RvlDevice not found.");
            return false;
        }

        Console.WriteLine($"RvlDevice: {_device.GetManufacturer()}:{_device.GetProductName()} SN: {_device.GetSerialNumber()} ({_device.VendorID:X4}:{_device.ProductID:X4})");

        _stream = _device.Open();

        if (_stream == null)
        {
            Console.WriteLine("Error: Failed to open RvlDevice stream.");
            return false;
        }

        Console.WriteLine("init: RvlDevice done.");

        return true;
    }

    public async Task SendData(RvlMonitorData data)
    {
        if (_device == null || _stream == null)
        {
            Console.WriteLine("Error: RvlDevice not initialized.");
            return;
        }

        if (_stream.CanWrite == false)
        {
            Console.WriteLine("Error: RvlDevice stream is not writable.");
            return;
        }

        byte[] report = new byte[Constants.Report.Length];
        report[Constants.Report.Index.ReportId] = Constants.Report.Null; // Report ID (0 for default)
        report[Constants.Report.Index.Type] = Constants.Report.DataPayload;
        report[Constants.Report.Index.CpuTemp] = (byte)data.CpuTemperature;
        report[Constants.Report.Index.CpuUtilization] = (byte)data.CpuUtilization;
        report[Constants.Report.Index.GpuTemp] = (byte)data.GpuTemperature;
        report[Constants.Report.Index.GpuUtilization] = (byte)data.GpuUtilization;

        await _stream.WriteAsync(report);
        // await _stream.FlushAsync();
    }

    public async Task SendCommand(byte command, byte value = Constants.Report.Null)
    {
        if (_device == null || _stream == null)
        {
            Console.WriteLine("Error: RvlDevice not initialized.");
            return;
        }

        if (_stream.CanWrite == false)
        {
            Console.WriteLine("Error: RvlDevice stream is not writable.");
            return;
        }

        byte[] report = new byte[Constants.Report.Length];
        report[Constants.Report.Index.ReportId] = Constants.Report.Null; // Report ID (0 for default)
        report[Constants.Report.Index.Type] = Constants.Report.CommandPayload;
        report[Constants.Report.Index.CommandName] = command;
        report[Constants.Report.Index.CommandValue] = value;

        await _stream.WriteAsync(report);
        await _stream.FlushAsync();
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _device = null;
        GC.SuppressFinalize(this);
    }

    public string GetDeviceInfo()
    {
        if (_device == null)
        {
            return "RvlDevice not found.";
        }

        return $"RvlDevice: {_device.GetManufacturer()}:{_device.GetProductName()} SN: {_device.GetSerialNumber()} ({_device.VendorID:X4}:{_device.ProductID:X4}) @ Stream: {(_stream != null && _stream.CanWrite ? "Open" : "Closed")} ";
    }
}