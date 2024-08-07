﻿using SystemInfoApi.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using LiteDB;
using Npgsql;
using RepoDb;
using Serilog;
using SystemInfoApi.Helpers;

namespace SystemInfoApi.Services
{
    public class SaveStatsPerSecond : IHostedService, IDisposable
    {
        private Timer _timer;

        public SaveStatsPerSecond()
        {
        }

        public Task StartAsync( CancellationToken stoppingToken )
        {
            _timer = new Timer( DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds( 1 ) );
            Log.Information( $"Save Stats per Second Hosted Service Started at {DateTime.Now}." );
            return Task.CompletedTask;
        }

        private async void DoWork( object state )
        {
            try
            {
                // Memory and Swap
                memory_metrics metrics = new memory_metrics();
                metrics = await MetricsHelper.GetMemoryMetricsAsync();

                Program.cbMemoryMetricsCollection.Add( metrics );

                // Drives
                List<drive_metrics> lstDrives = new List<drive_metrics>();
                lstDrives = await MetricsHelper.GetDrivesMetricsAsync();

                foreach ( var drive in lstDrives )
                {
                    Program.cbDrivesMetricsCollection.Add( drive );
                }

                // CPU 
                List<cpu_metrics> lstCpus = new List<cpu_metrics>();
                lstCpus = await MetricsHelper.GetCPUMetricsAsync();

                foreach ( var cpu in lstCpus )
                {
                    Program.cbCPUMetricsCollection.Add( cpu );
                }


#if DEBUG
                //Log.Information($"CPU Records: {Program.cbCPUMetricsCollection.Count}, Memory records: {Program.cbMemoryMetricsCollection.Count}, Drives Records: {Program.cbDrivesMetricsCollection.Count}");
#endif
            }
            catch ( Exception ex )
            {
                Log.Error( $"{ex}" );
            }
        }

        public Task StopAsync( CancellationToken stoppingToken )
        {
            _timer?.Change( Timeout.Infinite, 0 );
            Log.Information( $"Save Stats per Second Hosted Service Stopped at {DateTime.Now}" );
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}