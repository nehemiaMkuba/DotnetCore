using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Hangfire.Dashboard.BasicAuthorization;

using Quartz;
using Amazon.SQS;
using Amazon.Runtime;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.SwaggerGen;

using Core.Domain;
using Core.Management;
using IPN.API.Filters;
using Core.Domain.Enums;
using IPN.API.Attributes;
using Core.Domain.Entities;
using Core.Management.Common;
using IPN.API.Models.Common;

[assembly: ApiController]
namespace IPN.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration _configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">The collection of services to configure the application with.</param>        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddAuthentication(_configuration);
            services.AddApplication(_configuration);
            services.AddInfrastructure(_configuration);
            services.AddCustomHangfire(_configuration);
            services.AddAutoMapper(cfg => { cfg.AllowNullDestinationValues = true; cfg.AllowNullCollections = false; },   Assembly.GetAssembly(GetType()));
            services.AddCustomControllers();
            services.AddVersioning();
            services.AddSwaggerDocumentation();
            services.AddCustomOptions(_configuration);
            services.AddAmazonSQSClient(_configuration);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// Configures the application using the provided builder, hosting environment, and API version description provider.
        /// </summary>
        /// <param name="app">The current application builder.</param>
        /// <param name="env">The current hosting environment</param>
        /// <param name="provider">The API version descriptor provider used to enumerate defined API versions.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAllOrigins");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseSwaggerDocumentation(provider);

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                StatsPollingInterval = int.Parse(_configuration["Hangfire:StatsPollingInMs"]),//refresh dashboard every minute - default 2000
                IsReadOnlyFunc = (DashboardContext context) => bool.Parse(_configuration["Hangfire:IsReadOnlyFunc"]),
                AppPath = default, // no back link - back to site:baseurl
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions { RequireSsl = false, SslRedirect = false, LoginCaseSensitive = true,
                    Users = new[] { new BasicAuthAuthorizationUser { Login = "hangfire", PasswordClear = _configuration["Hangfire:HangfireDash"] } } }) }
            });

            app.UseHangfireServer();

            HangfireJobScheduler.ScheduleRecurringJobs();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public static class ConfigurationExtensionMethods
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            //Add Token Validation Parameters
            TokenValidationParameters tokenParameters = new TokenValidationParameters
            {
                //what to validate
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                //set up validation data
                ValidIssuer = configuration["Security:Issuer"],
                ValidAudience = configuration["Security:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Security:Key"])),
                ClockSkew = new TimeSpan(0)//The validation parameters have a default clock skew of 5 minutes so i have to invalidate it to 0
            };

            //Add JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenParameters;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(AuthPolicy.GlobalRights), policy => policy.RequireRole(nameof(Roles.Root), nameof(Roles.Admin), nameof(Roles.Webapi), nameof(Roles.User)));
                options.AddPolicy(nameof(AuthPolicy.ElevatedRights), policy => policy.RequireRole(nameof(Roles.Root), nameof(Roles.Admin)));
            });

            return services;
        }

        public static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            //REF https://dev.to/99darshan/restful-web-api-versioning-with-asp-net-core-1e8g
            //REF https://github.com/Microsoft/aspnet-api-versioning/wiki
            services.AddApiVersioning(options =>
            {
                // specify the default API Version as 1.0
                options.DefaultApiVersion = new ApiVersion(1, 0);

                // if the client hasn't specified the API version in the request, use the default API version number 
                options.AssumeDefaultVersionWhenUnspecified = true;

                // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;

                // DEFAULT Version reader is QueryStringApiVersionReader();
                // clients request the specific version using the X-version header
                // options.ApiVersionReader = new Microsoft.AspNetCore.Mvc.Versioning.HeaderApiVersionReader("X-version");   
                // Supporting multiple versioning scheme
                // options.ApiVersionReader = ApiVersionReader.Combine(new HeaderApiVersionReader(new[] { "api-version", "x-version", "version" }),
                // new QueryStringApiVersionReader(new[] { "api-version", "v", "version" }));//MediaTypeApiVersionReader-UrlSegmentApiVersionReader

                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ErrorResponses = new VersionErrorProvider();
            });

            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        public static IServiceCollection AddCustomHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config => config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), 
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),//max allowed execution time without status change
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true,
                    PrepareSchemaIfNecessary = true //if you want to install objects manually, or integrate it with your existing migration subsystem - install.sql is located at hangfire.sql.tools             
                }));            

            return services;
        }

        public static IServiceCollection AddCustomControllers(this IServiceCollection services)
        {
            //TODO: revisit porting to native system.text.json https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ModelStateFilter));
                options.Filters.Add(typeof(ExceptionFilter));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen(options =>
            {
                // add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                // integrate xml comments
                options.IncludeXmlComments(XmlCommentsFilePath);

                //define how the API is secured by defining one or more security schemes.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Enter in the value field: <b>Bearer {your JWT token}</b>"
                });

                options.OrderActionsBy(description =>
                {
                    ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)description.ActionDescriptor;
                    SwaggerOrderAttribute attribute = (SwaggerOrderAttribute)controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute(typeof(SwaggerOrderAttribute));
                    return string.IsNullOrEmpty(attribute?.Order?.Trim()) ? description.GroupName : attribute.Order.Trim();
                });

                //Operation security scheme based on Authorize attribute using OperationFilter()
                options.OperationFilter<SwaggerAuthOperationFilter>();
            });

            return services;
        }

        public static string XmlCommentsFilePath
        {
            get
            {
                //typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

            }
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.            
            app.UseSwagger();

            //Enable middleware to serve swagger - ui(HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                //options.RoutePrefix = "";
                // build a swagger endpoint for each discovered API version
                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
                options.DocExpansion(docExpansion: DocExpansion.None);
            });

            return app;
        }

        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {            
            services.Configure<EventSetting>(configuration.GetSection("Events"));
            return services;
        }

        public static IServiceCollection AddAmazonSQSClient(this IServiceCollection services, IConfiguration configuration)
        {
            //AmazonSQSConfig sqsConfig = new AmazonSQSConfig() { ServiceURL = configuration["Events:ServiceUrl"] };
            //services.AddSingleton(new AmazonSQSClient(new BasicAWSCredentials(configuration["Events:Key"], configuration["Events:Secret"]), sqsConfig));
            return services;
        }

        public static IServiceCollection AddQuartz(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });

            services.AddQuartz(config =>
            {
                config.UseMicrosoftDependencyInjectionJobFactory();
                // Register the job, loading the schedule from configuration
                //config.AddJobAndTrigger<NotificationHandlerJob>(configuration);
               
            });

            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });

            return services;

        }
    }

    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }

    public class HangfireJobScheduler
    {
        public static void ScheduleRecurringJobs()
        {
            //Uses NCrontab Format:sec(0-59),min(0-59),hour(0-23),day of month(1-31), month(1-12), day of week(0-6, 0 = sunday)
            //REF https://codeburst.io/schedule-background-jobs-using-hangfire-in-net-core-2d98eb64b196           

        }
    }
}
