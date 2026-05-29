using SystemMonitorApi.Models;

namespace SystemMonitorApi.Services;

public interface IRegistryReader
{
    IReadOnlyList<HwInfoSensorValue> ReadSensorValues();
}
