using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace RPSLSGameServiceAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // Set the minimum level of logs that will be captured
                .Enrich.FromLogContext() // Enrich with contextual information
                .WriteTo.File("logs/webapp-.log", rollingInterval: RollingInterval.Day) // Log to a rolling file
                .CreateLogger();

            try
            {
                Log.Information("Starting web host...");

                // Create and run the host
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush(); // Ensure all log events are flushed before application exits
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
