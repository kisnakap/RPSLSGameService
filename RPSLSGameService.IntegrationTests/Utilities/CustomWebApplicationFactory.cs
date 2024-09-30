using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RPSLSGameService.Infrastructure;
using System;
using System.Linq;

namespace RPSLSGameService.IntegrationTests.Utilities
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Ensure current DbContext options are replaced for testing.
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RPSLSDbContext>));
                services.Remove(descriptor);

                services.AddDbContext<RPSLSDbContext>(options =>
                {
                    options.UseSqlite("DataSource = RPSLSGameTest.db");
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<RPSLSDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                    db.Database.EnsureCreated(); // Ensure the database is created.

                    try
                    {
                        SeedData(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the database. Error: {Message}.", ex.Message);
                    }
                }
            });

            return base.CreateHost(builder);
        }

        private void SeedData(RPSLSDbContext db)
        {
        }
    }
}
