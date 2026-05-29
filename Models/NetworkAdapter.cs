namespace SystemMonitorApi.Models;

public class NetworkAdapter
{
    public string Name { get; set; } = string.Empty;
    public double? DownloadKbps { get; set; }
    public double? UploadKbps { get; set; }
}
