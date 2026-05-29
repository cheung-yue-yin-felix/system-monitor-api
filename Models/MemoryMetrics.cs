namespace SystemMonitorApi.Models;

public class MemoryMetrics
{
    public double? UsedMb { get; set; }
    public double? AvailableMb { get; set; }
    public double? UsagePercent { get; set; }
    public double? ClockMHz { get; set; }
    public double? Multiplier { get; set; }
}
