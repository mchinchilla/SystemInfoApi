﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SystemInfoApi.Services
{
    public class SaveStatsPerHour : IHostedService, IDisposable
    {
        private Timer _timer;

        public SaveStatsPerHour()
        {
        }

        public Task StartAsync( CancellationToken stoppingToken )
        {
            _timer = new Timer( DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes( 60 ) );
            Log.Information( $"Save Stats per Hour Hosted Service Started at {DateTime.Now}." );
            return Task.CompletedTask;
        }

        private void DoWork( object state )
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch ( Exception ex )
            {
                Log.Error( $"{ex}" );
            }
        }

        public Task StopAsync( CancellationToken stoppingToken )
        {
            _timer?.Change( Timeout.Infinite, 0 );
            Log.Information( $"Save Stats per Hour Hosted Service Stopped at {DateTime.Now}" );
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}