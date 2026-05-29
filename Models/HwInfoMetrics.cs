namespace SystemMonitorApi.Models;

public class HwInfoMetrics
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public CpuMetrics? Cpu { get; set; }
    public GpuMetrics? Gpu { get; set; }
    public MemoryMetrics? Memory { get; set; }
    public List<NetworkAdapter> Network { get; set; } = [];
    public List<SensorGroup>? Unmapped { get; set; }
}
