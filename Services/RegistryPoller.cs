using System.Runtime.CompilerServices;
using SystemMonitorApi.Models;

namespace SystemMonitorApi.Services;

public class RegistryPoller : IRegistryPoller
{
    private readonly IRegistryReader _reader;
    private readonly ISensorMapper _mapper;

    public RegistryPoller(IRegistryReader reader, ISensorMapper mapper)
    {
        _reader = reader;
        _mapper = mapper;
    }

    public async IAsyncEnumerable<HwInfoMetrics> StreamAsync(
        TimeSpan interval,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var raw = _reader.ReadSensorValues();
            yield return _mapper.MapToStructured(raw);

            try
            {
                await Task.Delay(interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }
    }
}
