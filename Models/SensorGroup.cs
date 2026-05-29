namespace SystemMonitorApi.Models;

public class SensorGroup
{
    public string Device { get; set; } = string.Empty;
    public List<SensorReading> Readings { get; set; } = [];
}
