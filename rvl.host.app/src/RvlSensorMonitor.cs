using LibreHardwareMonitor.Hardware;

namespace Rvl.Host.App;

public class RvlSensorMonitor : IRvlSensorMonitor, IDisposable
{
    private readonly Computer _computer;
    private IHardware? _cpu;
    private IHardware? _gpu;

    public RvlSensorMonitor()
    {
        _computer = new Computer()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            // IsMemoryEnabled = true,
            // IsMotherboardEnabled = true,
            // IsControllerEnabled = true,
            // IsNetworkEnabled = true,
            // IsStorageEnabled = true,
        };
    }

    public bool Initialize()
    {
        var hasError = false;

        Console.WriteLine("init: SensorMonitor");

        _computer.Open();

        if (_computer.Hardware == null || !_computer.Hardware.Any())
        {
            Console.WriteLine("Error: No hardware found.");
            return false;
        }

        _cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        _gpu = _computer.Hardware.FirstOrDefault(h => (h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel) && h.Sensors.Any(s => s.SensorType == SensorType.Fan && s.Name.Contains("GPU Fan")));

        if (_cpu == null)
        {
            Console.WriteLine("Error: CPU not found.");
            hasError = true;
        }

        if (_gpu == null)
        {
            Console.WriteLine("Error: GPU not found.");
            hasError = true;
        }

        Console.WriteLine("init: SensorMonitor done.");

        Console.WriteLine("CPU: {0}", _cpu?.Name ?? "Unknown");
        Console.WriteLine("GPU: {0}", _gpu?.Name ?? "Unknown");

        return !hasError;
    }

    public RvlMonitorData Poll()
    {
        var data = new RvlMonitorData();

        if (_cpu == null)
        {
            Console.WriteLine("Error: CPU not initialized.");
        }
        else
        {
            _cpu.Update();
            var cpuTemperatureSensor = _cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("Tctl/Tdie"));
            var cpuUtilizationSensor = _cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("CPU Total"));
            data.CpuTemperature = (int)(cpuTemperatureSensor?.Value ?? 0);
            data.CpuUtilization = (int)(cpuUtilizationSensor?.Value ?? 0);
            data.CpuName = _cpu.Name;
        }

        if (_gpu == null)
        {
            Console.WriteLine("Error: GPU not initialized.");
        }
        else
        {
            _gpu.Update();
            var gpuTemperatureSensor = _gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("GPU Core"));
            var gpuUtilizationSensor = _gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("GPU Core"));
            data.GpuTemperature = (int)(gpuTemperatureSensor?.Value ?? 0);
            data.GpuUtilization = (int)(gpuUtilizationSensor?.Value ?? 0);
            data.GpuName = _gpu.Name;
        }

        return data;
    }

    public void Dispose()
    {
        _computer.Close();
        _cpu = null;
        _gpu = null;
        GC.SuppressFinalize(this);
    }
}