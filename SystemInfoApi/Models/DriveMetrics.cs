using System;

namespace SystemInfoApi.Models
{
    [Serializable]
    public class DriveMetrics
    {
        public long? AvailableFreeSpace { get; set; }
        public string? DriveFormat { get; set; }
        public string? DriveType { get; set; }
        public bool IsReady { get; set; }
        public string Name { get; set; }
        public long? TotalFreeSpace { get; set; }
        public long? TotalSize { get; set; }
        public string? VolumeLabel { get; set; }
    }
}