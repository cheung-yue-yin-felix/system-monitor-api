namespace SystemMonitorApi.Models;

public class SensorRegistryOptions
{
    public const string SectionName = "Registry";

    public string Path { get; set; } = @"SOFTWARE\HWiNFO64\VSB";
}
