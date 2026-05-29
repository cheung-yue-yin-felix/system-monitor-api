using SystemMonitorApi.Models;

namespace SystemMonitorApi.Services;

public interface ISensorMapper
{
    HwInfoMetrics MapToStructured(IReadOnlyList<HwInfoSensorValue> rawValues);
}
