using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using SystemMonitorApi.Models;
using Microsoft.Extensions.Options;

namespace SystemMonitorApi.Services;

public partial class SensorMapper : ISensorMapper
{
    private readonly SensorMappingOptions _options;
    private readonly Dictionary<(Type Type, string Name), PropertyInfo?> _propertyCache = new();

    public SensorMapper(IOptions<SensorMappingOptions> options)
    {
        _options = options.Value;
    }

    public HwInfoMetrics MapToStructured(IReadOnlyList<HwInfoSensorValue> rawValues)
    {
        var metrics = new HwInfoMetrics();
        var unmappedRaw = new List<HwInfoSensorValue>();

        foreach (var raw in rawValues)
        {
            if (!TryParseStructuredLabel(raw.Label, out var category, out var metric))
            {
                unmappedRaw.Add(raw);
                continue;
            }

            if (!_options.Categories.TryGetValue(category, out var deviceType))
            {
                unmappedRaw.Add(raw);
                continue;
            }

            if (!_options.Fields.TryGetValue(deviceType, out var fieldMap) ||
                !TryGetPropertyMapping(fieldMap, metric, out var propertyName))
            {
                unmappedRaw.Add(raw);
                continue;
            }

            var target = GetOrCreateTarget(metrics, deviceType, raw.Sensor);
            if (target == null)
            {
                unmappedRaw.Add(raw);
                continue;
            }

            var value = ParseDouble(raw.ValueRaw);
            if (value.HasValue)
            {
                SetProperty(target, propertyName, value.Value);
            }
        }

        if (unmappedRaw.Count > 0)
        {
            metrics.Unmapped = unmappedRaw
                .GroupBy(r => r.Sensor)
                .Select(g => new SensorGroup
                {
                    Device = g.Key,
                    Readings = g.Select(MapReading).ToList()
                })
                .ToList();
        }

        return metrics;
    }

    private static bool TryParseStructuredLabel(string label, out string category, out string metric)
    {
        category = string.Empty;
        metric = string.Empty;

        if (string.IsNullOrWhiteSpace(label))
            return false;

        var match = StructuredLabelRegex().Match(label);
        if (!match.Success)
            return false;

        category = match.Groups[1].Value;
        metric = match.Groups[2].Value;
        return true;
    }

    private static bool TryGetPropertyMapping(Dictionary<string, string> fieldMap, string metric, out string propertyName)
    {
        propertyName = string.Empty;
        if (fieldMap.TryGetValue(metric, out var found))
        {
            propertyName = found;
            return true;
        }

        var match = fieldMap.FirstOrDefault(kvp =>
            kvp.Key.Equals(metric, StringComparison.OrdinalIgnoreCase));
        propertyName = match.Value;
        return propertyName != null;
    }

    private object? GetOrCreateTarget(HwInfoMetrics metrics, string deviceType, string sensorName)
    {
        var property = typeof(HwInfoMetrics).GetProperty(deviceType);
        if (property == null)
            return null;

        var propertyValue = property.GetValue(metrics);
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
        {
            // List type (e.g. Network -> List<NetworkAdapter>)
            var list = propertyValue;
            if (list == null)
            {
                list = Activator.CreateInstance(propertyType);
                property.SetValue(metrics, list);
            }

            var itemType = propertyType.GetGenericArguments()[0];
            var nameProp = itemType.GetProperty("Name");
            var name = ExtractDeviceName(sensorName);

            if (nameProp != null && list is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (nameProp.GetValue(item)?.ToString() == name)
                        return item;
                }
            }

            var newItem = Activator.CreateInstance(itemType);
            nameProp?.SetValue(newItem, name);
            propertyType.GetMethod("Add")?.Invoke(list, new[] { newItem });
            return newItem;
        }
        else
        {
            // Single nullable object (e.g. Cpu, Gpu, Memory)
            if (propertyValue == null)
            {
                propertyValue = Activator.CreateInstance(propertyType);
                property.SetValue(metrics, propertyValue);
            }

            var nameProp = propertyType.GetProperty("Name");
            if (nameProp != null)
            {
                var currentName = nameProp.GetValue(propertyValue)?.ToString() ?? string.Empty;
                var newName = ExtractDeviceName(sensorName);
                if (string.IsNullOrEmpty(currentName) || currentName.Length > newName.Length || currentName.Contains(':'))
                {
                    nameProp.SetValue(propertyValue, newName);
                }
            }

            return propertyValue;
        }
    }

    private void SetProperty(object target, string propertyName, double value)
    {
        var targetType = target.GetType();
        var cacheKey = (targetType, propertyName);

        if (!_propertyCache.TryGetValue(cacheKey, out var property))
        {
            property = targetType.GetProperty(propertyName);
            _propertyCache[cacheKey] = property;
        }

        if (property != null && property.PropertyType == typeof(double?))
        {
            property.SetValue(target, value);
        }
    }

    private static SensorReading MapReading(HwInfoSensorValue raw)
    {
        return new SensorReading
        {
            Label = raw.Label,
            NumericValue = ParseDouble(raw.ValueRaw),
            FormattedValue = raw.Value,
            Unit = ExtractUnit(raw.Value),
            Color = raw.Color
        };
    }

    private static double? ParseDouble(string value)
    {
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static string ExtractDeviceName(string sensor)
    {
        // Extract after first colon (e.g. "CPU [#0]: AMD Ryzen 5 5600")
        var idx = sensor.IndexOf(':');
        if (idx >= 0)
        {
            sensor = sensor[(idx + 1)..].Trim();
        }

        // Extract after last " - " for network adapters (e.g. "...NIC - Wi-Fi 2")
        var dashIdx = sensor.LastIndexOf(" - ");
        if (dashIdx >= 0)
        {
            sensor = sensor[(dashIdx + 3)..].Trim();
        }

        // Strip suffixes like ": Enhanced", ": Tctl"
        var colonIdx = sensor.IndexOf(':');
        if (colonIdx >= 0)
        {
            sensor = sensor[..colonIdx].Trim();
        }

        return sensor;
    }

    private static string ExtractUnit(string formattedValue)
    {
        if (string.IsNullOrWhiteSpace(formattedValue))
            return string.Empty;

        var match = UnitRegex().Match(formattedValue);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    [GeneratedRegex(@"^\[(.+?)\|(.+?)\]$")]
    private static partial Regex StructuredLabelRegex();

    [GeneratedRegex(@"[\d\.,\s]+\s*(.+)$")]
    private static partial Regex UnitRegex();
}
