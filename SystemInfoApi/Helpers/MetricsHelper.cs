using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SystemInfoApi.Models;
using Serilog;

namespace SystemInfoApi.Helpers
{
    public static class MetricsHelper
    {
        public static async Task<memory_metrics> GetMemoryMetricsAsync()
        {
            var output = "";

            try
            {
                var info = new ProcessStartInfo("free -m");
                info.FileName = "/bin/bash";
                info.Arguments = "-c \"free -m\"";
                info.RedirectStandardOutput = true;

                using (var process = Process.Start(info))
                {
                    output = process.StandardOutput.ReadToEnd();
                }

                #if DEBUG
                Log.Information($"\n{output}");
                #endif
                
                var lines = output.Split("\n");
                var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                var metrics = new memory_metrics();
                metrics.mem_total = double.Parse(memory[1]);
                metrics.mem_used = double.Parse(memory[2]);
                metrics.mem_free = double.Parse(memory[3]);
                metrics.mem_shared = double.Parse(memory[4]);
                metrics.mem_buffer = double.Parse(memory[5]);
                metrics.mem_available = double.Parse(memory[6]);

                var swap = lines[2].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                metrics.swap_total = double.Parse(swap[1]);
                metrics.swap_used = double.Parse(swap[2]);
                metrics.swap_free = double.Parse(swap[3]);
                metrics.current_stamp = DateTime.Now;

                return await Task.Run(() => metrics);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(() => new memory_metrics());
            }
        }

        public static async Task<List<drive_metrics>> GetDrivesMetricsAsync()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<drive_metrics> lstDrives = new List<drive_metrics>();

            try
            {
                foreach (var drive in allDrives)
                {
                    drive_metrics drvM = new drive_metrics
                    {
                        name = drive.Name,
                        drive_type = drive.DriveType.ToString(),
                        drive_format = drive.DriveFormat,
                        total_size = drive.TotalSize,
                        total_free_space = drive.TotalFreeSpace,
                        available_free_space = drive.AvailableFreeSpace,
                        volume_label = drive.VolumeLabel,
                        is_ready = drive.IsReady,
                        current_stamp = DateTime.Now
                    };
                    lstDrives.Add(drvM);
                }

                #if DEBUG
                /*
                 * TEST OUTPUT
                 */
                //List<DriveInfo> lstDrives = allDrives.ToList().FindAll(d => d.is_ready == true && d.drive_type== drive_type.Fixed && !string.IsNullOrEmpty(d.drive_format) && d.drive_format!="squashfs");
                foreach (var d in lstDrives.FindAll(c => c.is_ready == true))
                {
                    Log.Information($"{string.Empty.PadRight(80, '=')}");
                    Log.Information($"  Volume label: {d.volume_label}");
                    Log.Information($"  File system: {d.drive_format}");
                    Log.Information($"  File Type: {d.drive_type}");
                    Log.Information($"  Available space to current user:{d.available_free_space} bytes");

                    Log.Information($"  Total available space:          {d.total_free_space} bytes");
                    Log.Information($"  Total size of drive:            {d.total_size} bytes ");
                }
                #endif

                Log.Information($"{string.Empty.PadRight(80, '=')}");
                return await Task.Run(() => lstDrives);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(() => new List<drive_metrics>());
            }
        }


        public static async Task<List<cpu_metrics>> GetCPUMetricsAsync()
        {
            try
            {
                var output = "";

                var info = new ProcessStartInfo("ps -axfwww");
                info.FileName = "/bin/bash";
                info.Arguments = "-c \"cat /proc/stat | grep cpu\"";
                info.RedirectStandardOutput = true;

                using(var process = Process.Start(info))
                { 
                    output = process.StandardOutput.ReadToEnd();
                }

                var lines = output.Split("\n");
                List<cpu_metrics> lstCpus = new List<cpu_metrics>();
                foreach (var line in lines)
                {
                    var current_cpu = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (current_cpu.Length==0)
                    {
                        break;
                    }
                
                    #if DEBUG
                    if (!Regex.Match(current_cpu[0].ToString(), @"cpu\d+$").Success)
                    {
                        Log.Information($"CPU Totals: {current_cpu[0].ToString()}");
                        Log.Information($"\n{output}");
                    }
                    #endif
                    
                    cpu_metrics cpuMetric = new cpu_metrics
                    {
                        cpu = current_cpu[0].ToString(),
                        user = double.Parse(current_cpu[1]),
                        nice = double.Parse(current_cpu[2]),
                        system = double.Parse(current_cpu[3]),
                        idle = double.Parse(current_cpu[4]),
                        iowait = double.Parse(current_cpu[5]),
                        irq = double.Parse(current_cpu[6]),
                        softirq = double.Parse(current_cpu[7]),
                        steal = double.Parse(current_cpu[8]),
                        guest = double.Parse(current_cpu[9]),
                        guest_nice = double.Parse(current_cpu[10]),
                        current_stamp = DateTime.Now
                    };
                    lstCpus.Add(cpuMetric);
                }
                
                return await Task.Run(()=> lstCpus);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(()=> new List<cpu_metrics>());
            }
        }
    }
}