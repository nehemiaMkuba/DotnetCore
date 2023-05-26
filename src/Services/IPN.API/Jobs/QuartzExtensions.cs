using System;
using Microsoft.Extensions.Configuration;

using Quartz;

namespace IPN.API.Jobs
{
    public static class QuartzExtensions
    {
        public static void AddJobAndTrigger<T>(this IServiceCollectionQuartzConfigurator quartz, IConfiguration configuration) where T : IJob
        {
            // Use the name of the IJob as the appsettings.json key
            string jobName = typeof(T).Name;

            // Try and load the schedule from configuration
            string configKey = $"Quartz:{jobName}";
            string cronSchedule = configuration[configKey];

            // Some minor validation
            if (string.IsNullOrEmpty(cronSchedule))
            {
                throw new Exception($"No Quartz.NET Cron schedule found in configuration for {configKey} job");
            }

            // register the job as before
            JobKey jobKey = new(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .WithIdentity($"{jobName}-Trigger")
                .WithCronSchedule(cronSchedule, x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time")))
                .ForJob(jobKey)
                );
        }
    }
}
