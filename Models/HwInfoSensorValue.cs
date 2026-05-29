namespace SystemMonitorApi.Models;

public class HwInfoSensorValue
{
    public int Index { get; set; }
    public string Sensor { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueRaw { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
