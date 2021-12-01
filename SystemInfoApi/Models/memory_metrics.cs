using System;

namespace SystemInfoApi.Models
{
    [Serializable]
    public class memory_metrics
    {
        public long id { get; set; }
        public double mem_total { get; set; }
        public double mem_used { get; set; }
        public double mem_free { get; set; }
        public double mem_shared { get; set; }
        public double mem_buffer { get; set; }
        public double mem_available { get; set; }
        public double swap_total { get; set; }
        public double swap_used { get; set; }
        public double swap_free { get; set; }
        public DateTime current_stamp { get; set; }
    }
}