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
        public static async Task<MemoryMetrics> GetMemoryMetricsAsync()
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

                var metrics = new MemoryMetrics();
                metrics.memTotal = double.Parse(memory[1]);
                metrics.memUsed = double.Parse(memory[2]);
                metrics.memFree = double.Parse(memory[3]);
                metrics.memShared = double.Parse(memory[4]);
                metrics.memBuffer = double.Parse(memory[5]);
                metrics.memAvailable = double.Parse(memory[6]);

                var swap = lines[2].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                metrics.swapTotal = double.Parse(swap[1]);
                metrics.swapUsed = double.Parse(swap[2]);
                metrics.swapFree = double.Parse(swap[3]);
                metrics.CurrentTimeStamp = DateTime.Now;

                return await Task.Run(() => metrics);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(() => new MemoryMetrics());
            }
        }

        public static async Task<List<DriveMetrics>> GetDrivesMetricsAsync()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<DriveMetrics> lstDrives = new List<DriveMetrics>();

            try
            {
                foreach (var drive in allDrives)
                {
                    DriveMetrics drvM = new DriveMetrics
                    {
                        DriveType = drive.DriveType.ToString(),
                        DriveFormat = drive.DriveFormat,
                        TotalSize = drive.TotalSize,
                        TotalFreeSpace = drive.TotalFreeSpace,
                        AvailableFreeSpace = drive.AvailableFreeSpace,
                        VolumeLabel = drive.VolumeLabel,
                        IsReady = drive.IsReady,
                        CurrentTimeStamp = DateTime.Now
                    };
                    lstDrives.Add(drvM);
                }

                #if DEBUG
                /*
                 * TEST OUTPUT
                 */
                //List<DriveInfo> lstDrives = allDrives.ToList().FindAll(d => d.IsReady == true && d.DriveType== DriveType.Fixed && !string.IsNullOrEmpty(d.DriveFormat) && d.DriveFormat!="squashfs");
                foreach (var d in lstDrives.FindAll(c => c.IsReady == true))
                {
                    Log.Information($"{string.Empty.PadRight(80, '=')}");
                    Log.Information($"  Volume label: {d.VolumeLabel}");
                    Log.Information($"  File system: {d.DriveFormat}");
                    Log.Information($"  File Type: {d.DriveType}");
                    Log.Information($"  Available space to current user:{d.AvailableFreeSpace} bytes");

                    Log.Information($"  Total available space:          {d.TotalFreeSpace} bytes");
                    Log.Information($"  Total size of drive:            {d.TotalSize} bytes ");
                }
                #endif

                Log.Information($"{string.Empty.PadRight(80, '=')}");
                return await Task.Run(() => lstDrives);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(() => new List<DriveMetrics>());
            }
        }


        public static async Task<List<CpuMetrics>> GetCPUMetricsAsync()
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
                List<CpuMetrics> lstCpus = new List<CpuMetrics>();
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
                    
                    CpuMetrics cpuMetric = new CpuMetrics
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
                        CurrentTimeStamp = DateTime.Now
                    };
                    lstCpus.Add(cpuMetric);
                }
                
                return await Task.Run(()=> lstCpus);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(()=> new List<CpuMetrics>());
            }
        }
    }
}