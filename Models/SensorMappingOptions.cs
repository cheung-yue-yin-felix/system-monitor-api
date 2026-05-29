namespace SystemMonitorApi.Models;

public class SensorMappingOptions
{
    public const string SectionName = "SensorMapping";

    /// <summary>
    /// Maps label category (e.g. "CPU", "GPU", "RAM") to device type (e.g. "Cpu", "Gpu", "Memory", "Network").
    /// </summary>
    public Dictionary<string, string> Categories { get; set; } = new();

    /// <summary>
    /// Maps device type -> metric name -> model property name.
    /// Example: { "Cpu": { "Temp": "TemperatureC", "Usage": "UsagePercent" } }
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> Fields { get; set; } = new();
}
