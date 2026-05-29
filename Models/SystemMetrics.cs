namespace SystemMonitorApi.Models;

public class SystemMetrics
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<SensorGroup> Groups { get; set; } = [];
}
