using ConfirmationService.DataAccess.Context;
using ConfirmationService.DataAccess.Postgres.DomainEvents;
using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;
using ConfirmationService.WebApi.Common.Options;
using ConfirmationService.WebApi.DomainEvents;
using Confluent.SchemaRegistry;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using Refit;
using System.Reflection;

namespace ConfirmationService.WebApi.Common.Extensions
{
    public static class ServicesExtensions
    {
        static string ConfigNameConnectionStringPostgre = "PostgreSql";
        /// <summary>
        /// Расширение для настройки клиента schema registry
        /// </summary>
        public static void AddSchemaRegistryClient(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddSingleton<ISchemaRegistryClient>(sp =>
                new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = configuration.GetSection(nameof(ExternalServicesOptions)).GetValue<string>(nameof(ExternalServicesOptions.SchemaRegistryAddress))
                }));
        }

        /// <summary>
        /// Регистрация всех сервисов, находящихся в проекте
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterExecutingAsseblyServices(this IServiceCollection services)
        {
            var serviceTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(st => st.IsClass && !st.IsAbstract && st.Name.EndsWith("Service"));
            foreach (var serviceType in serviceTypes)
            {
                var interfaceType = serviceType.GetInterfaces()
                    .FirstOrDefault(it => it.Name == $"I{serviceType.Name}");
                if (interfaceType is not null)
                    services.AddScoped(interfaceType, serviceType);
            }
        }


        public static void UsePostgreSql(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ConfirmationDbContext>((options) =>
            {
                var connectionString = configuration.GetConnectionString(ConfigNameConnectionStringPostgre);
                if (connectionString is null)
                    throw new NotFoundException($"Connection string with name {ConfigNameConnectionStringPostgre} not found");
                options.UseNpgsql(connectionString);
            });
        }

        /// <summary>
        /// Регистрирует все мапперы из текущей сборки, имена которых заканчиваются на "Mapper".
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        //public static void RegisterMappers(this IServiceCollection services)
        //{
        //    var mappers = Assembly.GetExecutingAssembly()
        //        .GetTypes()
        //        .Where(st => st.IsClass && st.Name.EndsWith("Mapper"));
        //    foreach (var mapper in mappers)
        //    {
        //        if (mapper is not null)
        //            services.AddSingleton(mapper);
        //    }
        //}

        /// <summary>
        /// Связка классов опций приложения с данными из appsettings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ExternalServicesOptions>(configuration.GetSection(nameof(ExternalServicesOptions)));
        }

        /// <summary>
        /// Регистрирует Http клиенты для обращения к внешним сервисам через Refit
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <exception cref="NotFoundException">Базовый адрес в appsettings в секции ExternalServicesOptions не найден</exception>
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
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));
            }
        }
        public static IServiceCollection AddConfirmationDomainEvents(this IServiceCollection services)
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IDomainEventHandler<ConfirmationCreatedEvent>, ConfirmationCreatedEventHandler>();
            services.AddScoped<IDomainEventHandler<ConfirmationRevokedEvent>, ConfirmationRevokedEventHandler>();
            services.AddScoped<IDomainEventHandler<ConfirmationRespondEvent>, ConfirmationRespondEventHandler>();
            return services;
        }

    }
}
