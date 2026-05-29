namespace SystemMonitorApi.Models;

public class GpuMetrics
{
    public string Name { get; set; } = string.Empty;
    public double? TemperatureC { get; set; }
    public double? ClockMHz { get; set; }
    public double? MemoryClockMHz { get; set; }
    public double? UsagePercent { get; set; }
    public double? Usage3DPercent { get; set; }
    public double? VramUsageMb { get; set; }
    public double? Fps { get; set; }
}
