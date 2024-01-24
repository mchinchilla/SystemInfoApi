using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemInfoApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Npgsql;
using RepoDb;
using Serilog;

namespace SystemInfoApi.Services
{
    public class SaveStatsPerMinute : IHostedService, IDisposable
    {
        private Timer _timer;

        public SaveStatsPerMinute()
        {
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            Log.Information($"Save Stats per Minute Hosted Service Started at {DateTime.Now}.");
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            // CPU
            var cpuAggregates = Program.cbCPUMetricsCollection
                .GroupBy(l => new { l.cpu, l.current_stamp.Minute })
                .Select(cl => new cpu_metrics
                {
                    cpu = cl.Select(x => x.cpu).FirstOrDefault(),
                    user = cl.Select(x => x.user).Average(),
                    nice = cl.Select(x => x.nice).Average(),
                    system = cl.Select(x => x.system).Average(),
                    idle = cl.Select(x => x.idle).Average(),
                    iowait = cl.Select(x => x.iowait).Average(),
                    softirq = cl.Select(x => x.softirq).Average(),
                    steal = cl.Select(x => x.steal).Average(),
                    guest = cl.Select(x => x.guest).Average(),
                    guest_nice = cl.Select(x => x.guest_nice).Average(),
                    current_stamp = cl.Select(x => x.current_stamp).Max()
                }).ToList();

            // Memory
            var memoryAggregates = Program.cbMemoryMetricsCollection
                .GroupBy(l => new { l.current_stamp.Minute })
                .Select(cl => new memory_metrics()
                {
                    mem_total = cl.Select(x => x.mem_total).Average(),
                    mem_available = cl.Select(x => x.mem_available).Average(),
                    mem_buffer = cl.Select(x => x.mem_buffer).Average(),
                    mem_free = cl.Select(x => x.mem_free).Average(),
                    mem_shared = cl.Select(x => x.mem_shared).Average(),
                    mem_used = cl.Select(x => x.mem_used).Average(),
                    swap_free = cl.Select(x => x.swap_free).Average(),
                    swap_total = cl.Select(x => x.swap_total).Average(),
                    swap_used = cl.Select(x => x.swap_used).Average(),
                    current_stamp = cl.Select(x => x.current_stamp).Max()
                }).ToList();


            // Drives
            var drivesAggregates = Program.cbDrivesMetricsCollection.Where(d => d.is_ready == true && d.drive_type == "Fixed" && !string.IsNullOrEmpty(d.drive_format) && d.drive_format != "squashfs")
                .GroupBy(l => new { l.name, l.current_stamp.Minute })
                .Select(cl => new drive_metrics()
                {
                    name = cl.Select(x => x.name).FirstOrDefault(),
                    drive_format = cl.Select(x => x.drive_format).FirstOrDefault(),
                    drive_type = cl.Select(x => x.drive_type).FirstOrDefault(),
                    volume_label = cl.Select(x => x.volume_label).FirstOrDefault(),
                    is_ready = cl.Select(x => x.is_ready).FirstOrDefault(),
                    total_free_space = cl.Select(x => x.total_free_space).Average(),
                    available_free_space = cl.Select(x => x.available_free_space).Average(),
                    total_size = cl.Select(x => x.total_size).Average(),
                    current_stamp = cl.Select(x => x.current_stamp).Max()
                }).ToList();

            // Network (Interfaces)

            try
            {
                switch (Program.CurrentDatabase)
                {
                    case "SQLite":
                        using (var db = new SQLiteConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            db.InsertAll(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            cpuAggregates.Clear();

                            // Memory
                            db.InsertAll(memoryAggregates);
                            Program.cbMemoryMetricsCollection.Clear();
                            memoryAggregates.Clear();

                            //Drives
                            db.InsertAll(drivesAggregates);
                            Program.cbDrivesMetricsCollection.Clear();
                            drivesAggregates.Clear();
                        }

                        break;
                    case "Postgres":
                        using (var db = new NpgsqlConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            db.InsertAll(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            cpuAggregates.Clear();

                            // Memory
                            db.InsertAll(memoryAggregates);
                            Program.cbMemoryMetricsCollection.Clear();
                            memoryAggregates.Clear();

                            //Drives
                            db.InsertAll(drivesAggregates);
                            Program.cbDrivesMetricsCollection.Clear();
                            drivesAggregates.Clear();
                        }

                        break;
                    case "MySQL":
                        using (var db = new NpgsqlConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            db.InsertAll(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            cpuAggregates.Clear();

                            // Memory
                            db.InsertAll(memoryAggregates);
                            Program.cbMemoryMetricsCollection.Clear();
                            memoryAggregates.Clear();

                            //Drives
                            db.InsertAll(drivesAggregates);
                            Program.cbDrivesMetricsCollection.Clear();
                            drivesAggregates.Clear();
                        }

                        break;
                    case "SQLServer":
                        using (var db = new NpgsqlConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            db.InsertAll(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            cpuAggregates.Clear();

                            // Memory
                            db.InsertAll(memoryAggregates);
                            Program.cbMemoryMetricsCollection.Clear();
                            memoryAggregates.Clear();

                            //Drives
                            db.InsertAll(drivesAggregates);
                            Program.cbDrivesMetricsCollection.Clear();
                            drivesAggregates.Clear();
                        }

                        break;
                    case "LiteDB":
                        using (var db = new LiteDatabase($@"{Program.CurrentConnectionString}"))
                        {
                            var colCPU = db.GetCollection<cpu_metrics>("cpu_metrics");
                            Log.Information($"CPU Records By Minute: {cpuAggregates.Count}");
                            colCPU.InsertBulk(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            cpuAggregates.Clear();

                            // Memory
                            var colMem = db.GetCollection<memory_metrics>("memory_metrics");
                            colMem.InsertBulk(memoryAggregates);
                            Program.cbMemoryMetricsCollection.Clear();
                            memoryAggregates.Clear();

                            // Drives
                            var colDrvs = db.GetCollection<drive_metrics>("drive_metrics");
                            colDrvs.InsertBulk(drivesAggregates);
                            Program.cbDrivesMetricsCollection.Clear();
                            drivesAggregates.Clear();
                        }

                        break;
                    case "MongoDB":
                        // TO-DO
                        break;
                    case "RavenDB":
                        // TO-DO
                        break;
                    default:
                        using (var db = new SQLiteConnection(Program.CurrentConnectionString))
                        {
                            db.InsertAll(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            cpuAggregates.Clear();

                            // Memory
                            db.InsertAll(memoryAggregates);
                            Program.cbMemoryMetricsCollection.Clear();
                            memoryAggregates.Clear();

                            //Drives
                            db.InsertAll(drivesAggregates);
                            Program.cbDrivesMetricsCollection.Clear();
                            drivesAggregates.Clear();
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex}");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            Log.Information($"Save Stats per Minute Hosted Service Stopped at {DateTime.Now}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}