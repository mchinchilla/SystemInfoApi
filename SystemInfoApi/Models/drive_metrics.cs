using System;

namespace SystemInfoApi.Models
{
    [Serializable]
    public class drive_metrics
    {
        public long id { get; set; }
        public string name { get; set; }
        public string? drive_format { get; set; }
        public string? drive_type { get; set; }
        public string? volume_label { get; set; }
        public bool is_ready { get; set; }
        public double? total_free_space { get; set; }
        public double? available_free_space { get; set; }
        public double? total_size { get; set; }
        public DateTime current_stamp { get; set; }
    }
}