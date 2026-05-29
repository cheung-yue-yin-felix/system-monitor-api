using SystemMonitorApi.Models;

namespace SystemMonitorApi.Services;

public interface IDiskInfoReader
{
    IReadOnlyList<DiskMetric> ReadDiskMetrics();
}