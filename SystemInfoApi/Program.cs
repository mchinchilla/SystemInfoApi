using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using SystemInfoApi.Models;
using Serilog;
using Serilog.Events;

namespace SystemInfoApi
{
    public static class Program
    {
        public static ConcurrentBag<MemoryMetrics> cbMemoryMetricsCollection = new ConcurrentBag<MemoryMetrics>(); 
        public static ConcurrentBag<CpuMetrics> cbCPUMetricsCollection = new ConcurrentBag<CpuMetrics>(); 
        public static ConcurrentBag<DriveMetrics> cbDrivesMetricsCollection = new ConcurrentBag<DriveMetrics>();
        public static string CurrentDatabase { get; set; } = "LiteDB";
        public static string CurrentConnectionString { get; set; }

        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            try
            {
                Log.Information($"API starting ....");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
