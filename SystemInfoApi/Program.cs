using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
            
            // TEST CODE NETWORK INTERFACES INFO AND STATS
            try
            {
                IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                Console.WriteLine("Interface information for {0}.{1}     ",
                    computerProperties.HostName, computerProperties.DomainName);
                if (nics == null || nics.Length < 1)
                {
                    Console.WriteLine("  No network interfaces found.");
                }

                Console.WriteLine("  Number of interfaces .................... : {0}", nics.Length);

                foreach (NetworkInterface adapter in nics)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    Console.WriteLine();
                    Console.WriteLine(adapter.Description);
                    Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length,'='));
                    Console.WriteLine("  Adapter ID ................................ : {0}", adapter.Id);
                    Console.WriteLine("  Adapter Desc............................... : {0}", adapter.Description);
                    Console.WriteLine("  Adapter Name............................... : {0}", adapter.Name);
                    Console.WriteLine("  Interface type ............................ : {0}", adapter.NetworkInterfaceType);
                    Console.WriteLine("  Physical Address .......................... : {0}", adapter.GetPhysicalAddress().ToString());
                    Console.WriteLine("  Operational status ........................ : {0}", adapter.OperationalStatus);
                    string versions ="";

                    // Create a display string for the supported IP versions.
                    if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        versions = "IPv4";
                    }
                    if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                    {
                        if (versions.Length > 0)
                        {
                            versions += " ";
                        }
                        versions += "IPv6";
                    }
                    Console.WriteLine("  IP version ................................ : {0}", versions);                
                    // The following information is not useful for loopback adapters.
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }
                    Console.WriteLine("  DNS suffix ................................ : {0}", properties.DnsSuffix);

                    string label;
                    if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        IPv4InterfaceProperties ipv4 = properties.GetIPv4Properties();
                        Console.WriteLine("  MTU........................................ : {0}", ipv4.Mtu);
                        if (ipv4.UsesWins)
                        {

                            IPAddressCollection winsServers = properties.WinsServersAddresses;
                            if (winsServers.Count > 0)
                            {
                                label = "  WINS Servers .............................. :";
                            }
                        }
                    }

                    Console.WriteLine("  DNS enabled ............................... : {0}", properties.IsDnsEnabled);
                    Console.WriteLine("  Speed ..................................... : {0}", adapter.Speed);
                    Console.WriteLine("  Receive Only .............................. : {0}", adapter.IsReceiveOnly);
                    Console.WriteLine("  Multicast ................................. : {0}", adapter.SupportsMulticast);

                    var stats = adapter.GetIPv4Statistics();
                    Console.WriteLine("  Packet Received ........................... : {0}", stats.UnicastPacketsReceived);
                    Console.WriteLine("  Packet Sent ............................... : {0}", stats.UnicastPacketsSent);
                    Console.WriteLine("  Bytes Received ............................ : {0}", stats.BytesReceived);
                    Console.WriteLine("  Bytes Sent ................................ : {0}", stats.BytesSent);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error on Getting Info of Network Adapters: {ex.Message}");
            }

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
