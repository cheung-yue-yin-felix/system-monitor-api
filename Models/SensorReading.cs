namespace SystemMonitorApi.Models;

public class SensorReading
{
    public string Label { get; set; } = string.Empty;
    public double? NumericValue { get; set; }
    public string FormattedValue { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
