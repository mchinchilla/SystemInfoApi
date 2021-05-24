using System;

namespace SystemInfoApi.Models
{
    [Serializable]
    public class MemoryMetrics
    {
        public double Total;
        public double Used;
        public double Free;
        public double swapTotal { get; set; }
        public double swapUsed { get; set; }
        public double swapFree { get; set; }
    }
}