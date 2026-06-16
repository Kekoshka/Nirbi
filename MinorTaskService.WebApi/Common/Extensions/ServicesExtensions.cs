using Confluent.SchemaRegistry;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.WebApi.Common.DataSeed;
using MinorTaskService.WebApi.Common.ExternalApi;
using MinorTaskService.WebApi.Common.Options;
using Nirbi.ServiceAuth.Http;
using Refit;
using System.Reflection;

namespace MinorTaskService.WebApi.Common.Extensions
{
    public static class ServicesExtensions
    {
        static string ConfigNameConnectionStringPostgre = "PostgreSql";
        
        public static void UsePostgreSql(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>((serviceProvider,options) =>
            {
                var connectionString = configuration.GetConnectionString(ConfigNameConnectionStringPostgre);
                if (connectionString is null)
                    throw new NotFoundException($"Connection string with name {ConfigNameConnectionStringPostgre} not found");
                options.UseNpgsql(connectionString);
                options.UseSeeding((dbContext, _) =>
                {
                    var context = (AppDbContext)dbContext;

                    if ( context.Statuses.Any())
                        return;

                    context.Statuses.AddRange(StatusesSeed.Statuses);
                    context.SaveChanges();
                });
            });
        }

        public static void AddRefit(this IServiceCollection services, IConfiguration configuration)
        {
            var apiInterfaces = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(st => st.IsInterface && st.Name.StartsWith("I") && st.Name.EndsWith("Api"));
            foreach (var apiInterface in apiInterfaces)
            {
                var serviceName = apiInterface.Name.Substring(1, apiInterface.Name.Length - 4);
                var baseAddress = configuration
                    .GetSection(nameof(ExternalServicesOptions))
                    .GetValue<string>(serviceName + "Address");
                if (baseAddress is null)
                    throw new NotFoundException($"Base address for {serviceName} not found");

                services.AddRefitClient(apiInterface)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress))
                    .AddHttpMessageHandler<ServiceAccessTokenDelegatingHandler>();
            }
        }

        /// <summary>
        /// Расширение для настройки клиента schema registry
        /// </summary>
        public static void AddSchemaRegistryClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISchemaRegistryClient>(sp =>
                new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = configuration.GetSection(nameof(ExternalServicesOptions)).GetValue<string>(nameof(ExternalServicesOptions.SchemaRegistryAddress))
                }));
        }
    }
}
