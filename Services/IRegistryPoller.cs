using SystemMonitorApi.Models;

namespace SystemMonitorApi.Services;

public interface IRegistryPoller
{
    IAsyncEnumerable<HwInfoMetrics> StreamAsync(TimeSpan interval, CancellationToken cancellationToken = default);
}
