using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Npgsql;

namespace sampleapi
{
    internal class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static int Main()
        {
            try
            {
                logger.Info("lets rock");
                var p = new Parameters();
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(p.ConnectionString);
                var dataSource = dataSourceBuilder.Build();

                CreateWebHostBuilder(p)
                    .ConfigureServices(x =>
                    {
                        x.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(NpgsqlDataSource), dataSource));
                        x.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(Parameters), p));
                    })
                    .Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "something really bad happened");
                return 1;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        static IWebHostBuilder CreateWebHostBuilder(Parameters p)
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseUrls($"http://*:{p.Port}")
                .ConfigureKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMilliseconds(5000);
                    options.Limits.MaxRequestBodySize = 1024 * 1024;
                })
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging
                        .ClearProviders()
                        .SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();  // NLog: setup NLog for Dependency injection;

            return builder;
        }
    }
}
