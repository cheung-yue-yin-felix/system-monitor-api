using System.Runtime.Versioning;
using Microsoft.Win32;
using Microsoft.Extensions.Options;
using SystemMonitorApi.Models;

namespace SystemMonitorApi.Services;

[SupportedOSPlatform("windows")]
public class RegistryReader : IRegistryReader
{
    private readonly SensorRegistryOptions _options;

    public RegistryReader(IOptions<SensorRegistryOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<HwInfoSensorValue> ReadSensorValues()
    {
        var values = new List<HwInfoSensorValue>();

        using var key = Registry.CurrentUser.OpenSubKey(_options.Path);
        if (key is null)
        {
            return values;
        }

        // Discover all indices by looking at Sensor{i} entries
        var indexSet = new HashSet<int>();
        foreach (var name in key.GetValueNames())
        {
            if (name.StartsWith("Sensor") && int.TryParse(name[6..], out var idx))
            {
                indexSet.Add(idx);
            }
        }

        foreach (var index in indexSet.OrderBy(i => i))
        {
            var sensor = new HwInfoSensorValue
            {
                Index = index,
                Sensor = GetString(key, $"Sensor{index}"),
                Label = GetString(key, $"Label{index}"),
                Value = GetString(key, $"Value{index}"),
                ValueRaw = GetString(key, $"ValueRaw{index}"),
                Color = GetString(key, $"Color{index}")
            };

            values.Add(sensor);
        }

        return values;
    }

    private static string GetString(RegistryKey key, string name)
    {
        var value = key.GetValue(name);
        return value?.ToString() ?? string.Empty;
    }
}
