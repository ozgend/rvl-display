using LibreHardwareMonitor.Hardware;

namespace Rvl.Display.HWMonCli;

internal class Monitor
{
    private readonly Computer _computer;

    public Monitor()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsStorageEnabled = false,
            IsBatteryEnabled = false,
            IsNetworkEnabled = false,
            IsPsuEnabled = false,
        };
    }

    public void Run()
    {
        _computer.Open();
        _computer.Accept(new UpdateVisitor());

        foreach (IHardware hardware in _computer.Hardware)
        {
            Console.WriteLine($"Hardware: [{hardware.Name}]");

            foreach (IHardware subhardware in hardware.SubHardware)
            {
                Console.WriteLine($"\tSubhardware: [{subhardware.Name}]");

                foreach (ISensor sensor in subhardware.Sensors)
                {
                    Console.WriteLine($"\t\tSUB_HW Sensor: [{subhardware.Name}][{sensor.Name}] = ({sensor.Value})");
                }
            }

            foreach (ISensor sensor in hardware.Sensors)
            {
                Console.WriteLine($"\tHW Sensor: [{hardware.Name}][{sensor.Name}] = ({sensor.Value})");
            }
        }

        _computer.Close();
    }
}