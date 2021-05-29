using System;

namespace SystemInfoApi.Models
{
    [Serializable]
    public class MemoryMetrics
    {
        public double memTotal { get; set; }
        public double memUsed { get; set; }
        public double memFree { get; set; }
        public double memShared { get; set; }
        public double memBuffer { get; set; }
        public double memAvailable { get; set; }
        public double swapTotal { get; set; }
        public double swapUsed { get; set; }
        public double swapFree { get; set; }
        public DateTime CurrentTimeStamp { get; set; }
    }
}