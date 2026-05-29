namespace SystemMonitorApi.Models;

public class DiskMetric
{
    public string DiskName { get; set; } = string.Empty;
    public string DiskType { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public List<Partition> Partitions { get; set; } = [];
}

public class Partition
{
    public string DriveLetter { get; set; } = string.Empty;
    public double FreeSpaceGb { get; set; }
    public double UsedSpaceGb { get; set; }
}