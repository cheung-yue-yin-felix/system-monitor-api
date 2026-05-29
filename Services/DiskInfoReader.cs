using SystemMonitorApi.Models;
using DiskInfoToolkit;

namespace SystemMonitorApi.Services;

public class DiskInfoReader : IDiskInfoReader
{
    public IReadOnlyList<DiskMetric> ReadDiskMetrics()
    {
        var disks = Storage.GetDisks();
        var diskMetrics = new List<DiskMetric>();

        foreach (var storageDevice in disks)
        {
            var diskMetric = new DiskMetric
            {
                DiskName = storageDevice.DisplayName,
                DiskType = storageDevice.DeviceTypeLabel,
                Interface = storageDevice.BusType.ToString(),
                Partitions = []
            };

            foreach (var storageDevicePartition in storageDevice.Partitions)
            {
                if (storageDevicePartition.DriveLetter.HasValue)
                {
                    diskMetric.Partitions.Add(
                        new Partition
                        {
                            DriveLetter = storageDevicePartition.DriveLetter.ToString() ?? string.Empty, 
                            FreeSpaceGb = Convert.ToDouble(storageDevicePartition.AvailableFreeSpaceBytes/1024/1024/1024),
                            UsedSpaceGb = Convert.ToDouble((Convert.ToUInt64(storageDevicePartition.PartitionLength) - storageDevicePartition.AvailableFreeSpaceBytes)/1024/1024/1024)
                        }
                    );
                }
            }
            
            diskMetrics.Add(diskMetric);
        }
        
        return diskMetrics.AsReadOnly();
    }
}