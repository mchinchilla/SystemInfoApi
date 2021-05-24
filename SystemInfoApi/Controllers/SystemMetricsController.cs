using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SystemInfoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace SystemInfoApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class SystemMetrics : Controller
    {
        /// <summary>
        /// Returns the CPU metrics in the current Time (Realtime)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCpuMetrics()
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
                    Log.Information(output);
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
                
                    if (!Regex.Match(current_cpu[0].ToString(), @"cpu\d+$").Success)
                    {
                        Log.Information($"CPU Totals: {current_cpu[0].ToString()}");
                    }
                
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
                        guest_nice = double.Parse(current_cpu[10])
                    };
                    lstCpus.Add(cpuMetric);
                }
          
                return await Task.Run(()=> Ok(lstCpus));
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(()=> NotFound());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetMemoryMetrics()
        {
            var output = "";

            try
            {
                var info = new ProcessStartInfo("free -m");
                info.FileName = "/bin/bash";
                info.Arguments = "-c \"free -m\"";
                info.RedirectStandardOutput = true;

                using(var process = Process.Start(info))
                { 
                    output = process.StandardOutput.ReadToEnd();
                    Log.Information(output);
                }

                var lines = output.Split("\n");
                var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                var metrics = new MemoryMetrics();
                metrics.Total = double.Parse(memory[1]);
                metrics.Used = double.Parse(memory[2]);
                metrics.Free = double.Parse(memory[3]);
            
                var swap = lines[2].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                metrics.swapTotal = double.Parse(swap[1]);
                metrics.swapUsed = double.Parse(swap[2]);
                metrics.swapFree = double.Parse(swap[3]);
            
                return await Task.Run(()=> Ok(metrics));
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(()=> NotFound());
            }
        }

        /// <summary>
        /// Get the metric information of all Drives (Disks) in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDrivesMetrics()
        {
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                List<DriveMetrics> lstDrives = new List<DriveMetrics>();

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
                        IsReady =  drive.IsReady
                    };
                    lstDrives.Add(drvM);
                }
                
                /*
                 * TEST OUTPUT
                 */
                //List<DriveInfo> lstDrives = allDrives.ToList().FindAll(d => d.IsReady == true && d.DriveType== DriveType.Fixed && !string.IsNullOrEmpty(d.DriveFormat) && d.DriveFormat!="squashfs");
                foreach (var d in lstDrives.FindAll(c=>c.IsReady==true))
                {
                    Log.Information($"{string.Empty.PadRight(80,'=')}");
                    Log.Information($"  Volume label: {d.VolumeLabel}");
                    Log.Information($"  File system: {d.DriveFormat}");
                    Log.Information($"  File Type: {d.DriveType}");
                    Log.Information($"  Available space to current user:{d.AvailableFreeSpace} bytes");
                
                    Log.Information($"  Total available space:          {d.TotalFreeSpace} bytes");
                    Log.Information($"  Total size of drive:            {d.TotalSize} bytes ");
                }
                
                Log.Information($"{string.Empty.PadRight(80,'=')}");
                
                // var options = new JsonSerializerOptions()
                // {
                //     MaxDepth = 0,
                //     IgnoreNullValues = true,
                //     IgnoreReadOnlyProperties = true
                // };
                
                //var lstDrives1 = JsonSerializer.Serialize(lstDrives,options);
                
                return await Task.Run(()=> Ok(lstDrives.FindAll(c=>c.IsReady==true)));
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}\n{e}");
                return await Task.Run(()=> NotFound());
            }
        }
    }
}