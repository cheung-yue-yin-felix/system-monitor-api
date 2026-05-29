namespace SystemMonitorApi.Models;

public class CpuMetrics
{
    public string Name { get; set; } = string.Empty;
    public double? TemperatureC { get; set; }
    public double? UsagePercent { get; set; }
    public double? ClockMHz { get; set; }
    public double? PowerW { get; set; }
}
