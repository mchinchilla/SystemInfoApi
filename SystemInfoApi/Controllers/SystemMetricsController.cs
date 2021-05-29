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
using SystemInfoApi.Helpers;
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
                List<CpuMetrics> lstCpus = new List<CpuMetrics>();
                lstCpus = await MetricsHelper.GetCPUMetricsAsync();
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
            try
            {
                MemoryMetrics metrics = new MemoryMetrics();
                metrics = await MetricsHelper.GetMemoryMetricsAsync();
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
                List<DriveMetrics> lstDrives = new List<DriveMetrics>();
                lstDrives = await MetricsHelper.GetDrivesMetricsAsync();
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