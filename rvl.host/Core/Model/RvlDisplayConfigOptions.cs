using System.ComponentModel.DataAnnotations;

namespace Rvl.Display.Core.Model;

public sealed class RvlDisplayConfigOptions
{
    [Range(100, int.MaxValue, ErrorMessage = "SensorPollIntervalMs must be at least 100ms.")]
    public int SensorPollIntervalMs { get; set; } = Constants.SensorPollIntervalMs;

    public required CpuOptions Cpu { get; set; }
    public required GpuOptions Gpu { get; set; }
    public required MotherboardOptions Motherboard { get; set; }

    public sealed class CpuOptions
    {
        [Required(ErrorMessage = "Cpu.Temperature sensor name is required.")]
        public required string Temperature { get; set; }
        [Required(ErrorMessage = "Cpu.Utilization sensor name is required.")]
        public required string Utilization { get; set; }
        [Required(ErrorMessage = "Cpu.Fan sensor name is required.")]
        public required string Fan { get; set; }
    }

    public sealed class GpuOptions
    {
        [Required(ErrorMessage = "Gpu.Temperature sensor name is required.")]
        public required string Temperature { get; set; }
        [Required(ErrorMessage = "Gpu.Utilization sensor name is required.")]
        public required string Utilization { get; set; }
        [Required(ErrorMessage = "Gpu.Fan sensor name is required.")]
        public required string Fan { get; set; }
    }

    public sealed class MotherboardOptions
    {
        [Required(ErrorMessage = "Motherboard.ChasisFan sensor name is required.")]
        public required string ChasisFan { get; set; }
    }

    public override string ToString()
    {
        return $"Configuration:\n" +
            $"SensorPollIntervalMs: {SensorPollIntervalMs}\n" +
            $"Cpu: Temperature: [{Cpu.Temperature}], Utilization: [{Cpu.Utilization}], Fan: [{Cpu.Fan}]\n" +
            $"Gpu: Temperature: [{Gpu.Temperature}], Utilization: [{Gpu.Utilization}], Fan: [{Gpu.Fan}]\n" +
            $"Motherboard: ChasisFan: [{Motherboard.ChasisFan}]";
    }
}