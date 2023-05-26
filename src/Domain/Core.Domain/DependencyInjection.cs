using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Core.Domain.Enums;
using Core.Domain.Infrastructure.Database;
using Core.Domain.Infrastructure.Services;

namespace Core.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IPNContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(IPNContext).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.MigrationsHistoryTable("__GospelMigrationsHistory", nameof(Schemas.IPN));                    
                }));

            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddScoped<IIPNContext>(provider => provider.GetService<IPNContext>());
            services.AddScoped<IConnection>(_ => new Connection(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
