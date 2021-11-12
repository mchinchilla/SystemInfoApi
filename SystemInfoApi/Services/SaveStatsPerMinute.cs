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
using Microsoft.Data.Sqlite;
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
            _timer = new Timer(DoWork, null, TimeSpan.Zero,TimeSpan.FromSeconds(60));
            Log.Information($"Save Stats per Minute Hosted Service Started at {DateTime.Now}.");
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                switch (Program.CurrentDatabase)
                {
                    case "SQLite":
                        using (var db = new SQLiteConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            
                            db.InsertAll(Program.cbCPUMetricsCollection);
                            Program.cbCPUMetricsCollection.Clear();
                            
                            // Memory
                            db.InsertAll(Program.cbMemoryMetricsCollection);
                            Program.cbMemoryMetricsCollection.Clear();
                            
                            //Drives
                            db.InsertAll(Program.cbDrivesMetricsCollection);
                            Program.cbDrivesMetricsCollection.Clear();
                        }
                        
                        break;
                    case "Postgres":
                        using (var db = new NpgsqlConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            db.InsertAll(Program.cbCPUMetricsCollection);
                            Program.cbCPUMetricsCollection.Clear();
                            
                            // Memory
                            db.InsertAll(Program.cbMemoryMetricsCollection);
                            Program.cbMemoryMetricsCollection.Clear();
                            
                            //Drives
                            db.InsertAll(Program.cbDrivesMetricsCollection);
                            Program.cbDrivesMetricsCollection.Clear();
                        }
                        break;
                    case "MySQL":
                        using (var db = new NpgsqlConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            db.InsertAll(Program.cbCPUMetricsCollection);
                            Program.cbCPUMetricsCollection.Clear();
                            
                            // Memory
                            db.InsertAll(Program.cbMemoryMetricsCollection);
                            Program.cbMemoryMetricsCollection.Clear();
                            
                            //Drives
                            db.InsertAll(Program.cbDrivesMetricsCollection);
                            Program.cbDrivesMetricsCollection.Clear();
                        }
                        break;
                    case "SQLServer":
                        using (var db = new NpgsqlConnection(Program.CurrentConnectionString))
                        {
                            // CPU
                            db.InsertAll(Program.cbCPUMetricsCollection);
                            Program.cbCPUMetricsCollection.Clear();
                            
                            // Memory
                            db.InsertAll(Program.cbMemoryMetricsCollection);
                            Program.cbMemoryMetricsCollection.Clear();
                            
                            //Drives
                            db.InsertAll(Program.cbDrivesMetricsCollection);
                            Program.cbDrivesMetricsCollection.Clear();
                        }
                        break;
                    case "LiteDB":
                        using (var db = new LiteDatabase($@"{Program.CurrentConnectionString}"))
                        {
                            // CPU
                            var colCPU = db.GetCollection<CpuMetrics>("CpuMetrics");
                            //colCPU.InsertBulk(Program.cbCPUMetricsCollection);
                            var cpuAggregates = Program.cbCPUMetricsCollection
                                .GroupBy(l => new {l.cpu, l.CurrentTimeStamp.Minute})
                                .Select(cl => new CpuMetrics
                                {
                                    cpu = cl.Select(x=>x.cpu).FirstOrDefault(),
                                    user = cl.Select(x=>x.user).Average(),
                                    nice = cl.Select(x=>x.nice).Average(),
                                    system = cl.Select(x=>x.system).Average(),
                                    idle = cl.Select(x=>x.idle).Average(),
                                    iowait = cl.Select(x=>x.iowait).Average(),
                                    softirq = cl.Select(x=>x.softirq).Average(),
                                    steal = cl.Select(x=>x.steal).Average(),
                                    guest = cl.Select(x=>x.guest).Average(),
                                    guest_nice = cl.Select(x=>x.guest_nice).Average(),
                                    CurrentTimeStamp = cl.Select(x=>x.CurrentTimeStamp).Max()
                                }).ToList();
                            Log.Information($"CPU Records By Minute: {cpuAggregates.Count}");
                            colCPU.InsertBulk(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            
                            // Memory
                            var colMem = db.GetCollection<MemoryMetrics>("MemoryMetrics");
                            colMem.InsertBulk(Program.cbMemoryMetricsCollection);
                            Program.cbMemoryMetricsCollection.Clear();
                            
                            // Drives
                            var colDrvs = db.GetCollection<DriveMetrics>("DriveMetrics");
                            colDrvs.InsertBulk(Program.cbDrivesMetricsCollection);
                            Program.cbDrivesMetricsCollection.Clear();
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
                            // CPU -- Test
                            var cpuAggregates = Program.cbCPUMetricsCollection
                                .GroupBy(l => new {l.cpu, l.CurrentTimeStamp.Minute})
                                .Select(cl => new CpuMetrics
                                {
                                    cpu = cl.Select(x=>x.cpu).FirstOrDefault(),
                                    user = cl.Select(x=>x.user).Average(),
                                    nice = cl.Select(x=>x.nice).Average(),
                                    system = cl.Select(x=>x.system).Average(),
                                    idle = cl.Select(x=>x.idle).Average(),
                                    iowait = cl.Select(x=>x.iowait).Average(),
                                    softirq = cl.Select(x=>x.softirq).Average(),
                                    steal = cl.Select(x=>x.steal).Average(),
                                    guest = cl.Select(x=>x.guest).Average(),
                                    guest_nice = cl.Select(x=>x.guest_nice).Average(),
                                    CurrentTimeStamp = cl.Select(x=>x.CurrentTimeStamp).Max()
                                }).ToList();
                            db.InsertAll(cpuAggregates);
                            Program.cbCPUMetricsCollection.Clear();
                            cpuAggregates.Clear();
                            
                            // Memory
                            db.InsertAll(Program.cbMemoryMetricsCollection);
                            Program.cbMemoryMetricsCollection.Clear();
                            
                            //Drives
                            db.InsertAll(Program.cbDrivesMetricsCollection);
                            Program.cbDrivesMetricsCollection.Clear();
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
