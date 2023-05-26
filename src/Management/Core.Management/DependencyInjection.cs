using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;

using IdGen;
using Dapper;

using Core.Management.Interfaces;
using Core.Management.Repositories;
using Core.Management.Infrastructure.Seedwork;
using Core.Management.Infrastructure.IntegrationEvents.EventBus;
using Core.Management.Infrastructure.IntegrationEvents.EventHandling;


namespace Core.Management
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddTransient<IIdGenerator<long>>(_ => new IdGenerator(0, new IdGeneratorOptions(idStructure: new IdStructure(45, 2, 16), timeSource: new DefaultTimeSource(new DateTime(2021, 4, 23, 11, 0, 0, DateTimeKind.Utc)))));
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IDataServiceFactory<>), typeof(DataServiceFactory<>));
            services.AddScoped<ISecurityRepository, SecurityRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IQueueService, QueueService>();
            services.AddScoped<ISeed, Seed>();
            SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
            return services;
        }
    }
}