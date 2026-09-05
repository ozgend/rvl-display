using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rvl.Display.Core.Interfaces;
using Rvl.Display.Core.Model;

namespace Rvl.Display.Core.Services;

public class RvlSensorMonitor : IRvlSensorMonitor, IDisposable
{
    private readonly IOptionsMonitor<RvlDisplayConfigOptions> _options;
    private readonly ILogger<RvlSensorMonitor>? _logger;

    private IHardware? _cpu;
    private IHardware? _gpu;
    private IHardware? _motherboard;
    private ISensor? _cpuTemperatureSensor;
    private ISensor? _cpuUtilizationSensor;
    private ISensor? _cpuFanSensor;
    private ISensor? _gpuTemperatureSensor;
    private ISensor? _gpuUtilizationSensor;
    private ISensor? _gpuFanSensor;
    private ISensor? _motherboardTemperatureSensor;
    private ISensor? _chassisFanSensor;
    private bool _isCpuErrorLogged;
    private bool _isGpuErrorLogged;
    private bool _isMotherboardErrorLogged;

    public Computer Computer { get; internal set; }

    public RvlSensorMonitor(IOptionsMonitor<RvlDisplayConfigOptions> options, ILogger<RvlSensorMonitor>? logger = null)
    {
        _options = options;
        _logger = logger;

        Computer = new Computer()
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

        Computer.Open();

        if (Computer.Hardware == null || !Computer.Hardware.Any())
        {
            _logger?.LogError("No hardware found.");
            return false;
        }

        _cpu = Computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        _gpu = Computer.Hardware.FirstOrDefault(h => (h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel) && h.Sensors.Any(s => s.SensorType == SensorType.Fan && s.Name.Contains(_options.CurrentValue.Gpu.Fan, StringComparison.OrdinalIgnoreCase)));
        _motherboard = Computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Motherboard);

        _cpuTemperatureSensor = FindSensor(_cpu, SensorType.Temperature, _options.CurrentValue.Cpu.Temperature);
        _cpuUtilizationSensor = FindSensor(_cpu, SensorType.Load, _options.CurrentValue.Cpu.Utilization);
        _cpuFanSensor = FindSensor(_cpu, SensorType.Fan, _options.CurrentValue.Cpu.Fan);
        _gpuTemperatureSensor = FindSensor(_gpu, SensorType.Temperature, _options.CurrentValue.Gpu.Temperature);
        _gpuUtilizationSensor = FindSensor(_gpu, SensorType.Load, _options.CurrentValue.Gpu.Utilization);
        _gpuFanSensor = FindSensor(_gpu, SensorType.Fan, _options.CurrentValue.Gpu.Fan);
        _motherboardTemperatureSensor = FindSensor(_motherboard, SensorType.Temperature, _options.CurrentValue.Motherboard.Temperature);
        _chassisFanSensor = FindSensor(_motherboard, SensorType.Fan, _options.CurrentValue.Motherboard.ChassisFan);

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

    public RvlMonitorData Poll(CancellationToken ct = default)
    {
        var dataStruct = RvlMonitorData.ZeroStruct();

        if (_cpu == null)
        {
            if (!_isCpuErrorLogged)
            {
                _logger?.LogError("Error: CPU not initialized.");
                _isCpuErrorLogged = true;
            }
        }
        else
        {
            _cpu.Update();
            dataStruct.CpuTemp = (byte)(_cpuTemperatureSensor?.Value ?? 0);
            dataStruct.CpuUtil = (byte)(_cpuUtilizationSensor?.Value ?? 0);
            dataStruct.CpuFan = (ushort)(_cpuFanSensor?.Value ?? 0);
        }

        if (_gpu == null)
        {
            if (!_isGpuErrorLogged)
            {
                _logger?.LogError("Error: GPU not initialized.");
                _isGpuErrorLogged = true;
            }
        }
        else
        {
            _gpu.Update();
            dataStruct.GpuTemp = (byte)(_gpuTemperatureSensor?.Value ?? 0);
            dataStruct.GpuUtil = (byte)(_gpuUtilizationSensor?.Value ?? 0);
            dataStruct.GpuFan = (ushort)(_gpuFanSensor?.Value ?? 0);
        }

        if (_motherboard == null)
        {
            if (!_isMotherboardErrorLogged)
            {
                _logger?.LogError("Error: Motherboard not initialized.");
                _isMotherboardErrorLogged = true;
            }
        }
        else
        {
            _motherboard.Update();
            dataStruct.ChassisTemp = (byte)(_motherboardTemperatureSensor?.Value ?? 0);
            dataStruct.ChassisFan = (ushort)(_chassisFanSensor?.Value ?? 0);
        }

        var data = new RvlMonitorData
        {
            Data = dataStruct,
            CpuName = _cpu?.Name ?? string.Empty,
            GpuName = _gpu?.Name ?? string.Empty,
            MotherboardName = _motherboard?.Name ?? string.Empty
        };

        return data;
    }

    public void Dispose()
    {
        Computer.Close();
        _cpu = null;
        _gpu = null;
        _motherboard = null;
        _cpuTemperatureSensor = null;
        _cpuUtilizationSensor = null;
        _cpuFanSensor = null;
        _gpuTemperatureSensor = null;
        _gpuUtilizationSensor = null;
        _gpuFanSensor = null;
        _motherboardTemperatureSensor = null;
        _chassisFanSensor = null;
        GC.SuppressFinalize(this);
    }

    private static ISensor? FindSensor(IHardware? hardware, SensorType sensorType, string name)
    {
        return hardware?.Sensors.FirstOrDefault(sensor =>
            sensor.SensorType == sensorType &&
            sensor.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }
}
