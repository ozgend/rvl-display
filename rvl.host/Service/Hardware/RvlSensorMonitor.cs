using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;

namespace Rvl.Display.Service.Hardware;

public class RvlSensorMonitor : IRvlSensorMonitor, IDisposable
{
    private readonly IOptionsMonitor<RvlDisplayConfigOptions> _options;
    private readonly Computer _computer;
    private readonly ILogger<RvlSensorMonitor>? _logger;

    private IHardware? _cpu;
    private IHardware? _gpu;
    private IHardware? _motherboard;

    public RvlSensorMonitor(IOptionsMonitor<RvlDisplayConfigOptions> options, ILogger<RvlSensorMonitor>? logger = null)
    {
        _options = options;
        _logger = logger;

        _computer = new Computer()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMotherboardEnabled = true,
            // IsMemoryEnabled = true,
            // IsControllerEnabled = true,
            // IsNetworkEnabled = true,
            // IsStorageEnabled = true,
        };
    }

    public bool Initialize()
    {
        var hasError = false;

        _logger?.LogInformation("init: SensorMonitor");
        _logger?.LogInformation("SensorMonitor config: {@Config}", _options.CurrentValue);

        _computer.Open();

        if (_computer.Hardware == null || !_computer.Hardware.Any())
        {
            _logger?.LogError("No hardware found.");
            return false;
        }

        _cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        _gpu = _computer.Hardware.FirstOrDefault(h => (h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel) && h.Sensors.Any(s => s.SensorType == SensorType.Fan && s.Name.Contains(_options.CurrentValue.Gpu.Fan, StringComparison.OrdinalIgnoreCase)));
        _motherboard = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Motherboard);

        if (_cpu == null)
        {
            _logger?.LogError("CPU not found.");
            hasError = true;
        }

        if (_gpu == null)
        {
            _logger?.LogError("GPU not found.");
            hasError = true;
        }

        _logger?.LogInformation("init: SensorMonitor done.");

        _logger?.LogInformation("CPU: {CpuName}", _cpu?.Name ?? "Unknown");
        _logger?.LogInformation("GPU: {GpuName}", _gpu?.Name ?? "Unknown");
        _logger?.LogInformation("Motherboard: {MotherboardName}", _motherboard?.Name ?? "Unknown");

        return !hasError;
    }

    public Task<RvlMonitorData> Poll(CancellationToken ct = default)
    {
        var dataStruct = RvlMonitorData.ZeroStruct();

        if (_cpu == null)
        {
            _logger?.LogError("Error: CPU not initialized.");
        }
        else
        {
            _cpu.Update();
            var cpuTemperatureSensor = _cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains(_options.CurrentValue.Cpu.Temperature, StringComparison.OrdinalIgnoreCase));
            var cpuUtilizationSensor = _cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains(_options.CurrentValue.Cpu.Utilization, StringComparison.OrdinalIgnoreCase));
            var cpuFanSensor = _cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Fan && s.Name.Contains(_options.CurrentValue.Cpu.Fan, StringComparison.OrdinalIgnoreCase));
            dataStruct.CpuTemp = (byte)(cpuTemperatureSensor?.Value ?? 0);
            dataStruct.CpuUtil = (byte)(cpuUtilizationSensor?.Value ?? 0);
            dataStruct.CpuFan = (ushort)(cpuFanSensor?.Value ?? 0);
        }

        if (_gpu == null)
        {
            _logger?.LogError("Error: GPU not initialized.");
        }
        else
        {
            _gpu.Update();
            var gpuTemperatureSensor = _gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains(_options.CurrentValue.Gpu.Temperature, StringComparison.OrdinalIgnoreCase));
            var gpuUtilizationSensor = _gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains(_options.CurrentValue.Gpu.Utilization, StringComparison.OrdinalIgnoreCase));
            var gpuFanSensor = _gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Fan && s.Name.Contains(_options.CurrentValue.Gpu.Fan, StringComparison.OrdinalIgnoreCase));
            dataStruct.GpuTemp = (byte)(gpuTemperatureSensor?.Value ?? 0);
            dataStruct.GpuUtil = (byte)(gpuUtilizationSensor?.Value ?? 0);
            dataStruct.GpuFan = (ushort)(gpuFanSensor?.Value ?? 0);
        }

        if (_motherboard == null)
        {
            _logger?.LogError("Error: Motherboard not initialized.");
        }
        else
        {
            _motherboard.Update();
            var chasisFanSensor = _motherboard.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Fan && s.Name.Contains(_options.CurrentValue.Motherboard.ChasisFan, StringComparison.OrdinalIgnoreCase));
            dataStruct.ChasisFan = (ushort)(chasisFanSensor?.Value ?? 0);
        }

        var data = new RvlMonitorData
        {
            Data = dataStruct,
            CpuName = _cpu?.Name ?? string.Empty,
            GpuName = _gpu?.Name ?? string.Empty
        };

        return Task.FromResult(data);
    }

    public void Dispose()
    {
        _computer.Close();
        _cpu = null;
        _gpu = null;
        _motherboard = null;
        GC.SuppressFinalize(this);
    }
}
