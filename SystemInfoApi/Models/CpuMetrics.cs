using System;

namespace SystemInfoApi.Models
{
    [Serializable]
    public class CpuMetrics
    {
        public string cpu { get; set; }
        public double user { get; set; }
        public double nice { get; set; }
        public double system { get; set; }
        public double idle { get; set; }
        public double iowait { get; set; }
        public double irq { get; set; }
        public double softirq { get; set; }
        public double steal { get; set; }
        public double guest { get; set; }
        public double guest_nice { get; set; }
        public DateTime CurrentTimeStamp { get; set; }
    }
}