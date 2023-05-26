using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Sentry.Extensions.Logging;

using Core.Domain.Infrastructure.Database;
using Core.Management.Infrastructure.Seedwork;
using Core.Management.Infrastructure.IntegrationEvents.EventHandling;

namespace IPN.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    IPNContext context = services.GetRequiredService<IPNContext>();
                    if (context.Database.IsSqlServer()) { context.Database.Migrate(); }

                    ISeed seed = services.GetRequiredService<ISeed>();
                    seed.SeedDefaults().Wait();
                }
                catch (Exception)
                {
                    logger.LogError("An error while setting up infrastructure - migration, sequences and seed");
                    throw;
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSentry(options =>
                    {
                        options.AddLogEntryFilter((category, level, eventId, exception) => category.StartsWith("Microsoft") && level < LogLevel.Error);
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
