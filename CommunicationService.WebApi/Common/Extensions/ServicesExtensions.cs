using CommunicationService.DataAccess.Postgres.Context;
using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.WebApi.Common.DataSeed;
using CommunicationService.WebApi.Common.Options;
using CommunicationService.WebApi.DomainEvents.Handlers;
using CommunicationService.WebApi.Services.BackgroundServices;
using Confluent.SchemaRegistry;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using Nirbi.ServiceAuth.Http;
using Refit;
using System.Reflection;

namespace CommunicationService.WebApi.Common.Extensions
{
    public static class ServicesExtensions
    {
        static string ConfigNameConnectionStringPostgre = "PostgreSql";

        public static void UsePostgreSql(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>((options) =>
            {
                var connectionString = configuration.GetConnectionString(ConfigNameConnectionStringPostgre);
                if (connectionString is null)
                    throw new NotFoundException($"Connection string with name {ConfigNameConnectionStringPostgre} not found");
                options.UseNpgsql(connectionString);
                options.UseSeeding((dbContext, _) =>
                {
                    var context = (AppDbContext)dbContext;

                    if (context.ChatTypes.Any())
                        return;

                    context.ChatTypes.AddRange(ChatTypesSeed.ChatTypes);
                    context.SaveChanges();
                });
            });
        }

        public static IServiceCollection AddDomainEvents(this IServiceCollection services)
        {
            services.AddHostedService<TaskParticipantAddedConsumer>();
            services.AddHostedService<TaskParticipantRemovedConsumer>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IDomainEventHandler<ChatCreatedEvent>, ChatCreatedEventHandler>();
            services.AddScoped<IDomainEventHandler<MessageCreatedEvent>, MessageCreatedEventHandler>();
            services.AddScoped<IDomainEventHandler<MessageDeletedEvent>, MessageDeletedEventHandler>();
            services.AddScoped<IDomainEventHandler<MessageUpdatedEvent>, MessageUpdatedEventHandler>();
            services.AddScoped<IDomainEventHandler<UserJoinedEvent>, UserJoinedEventHandler>();
            services.AddScoped<IDomainEventHandler<UserRemovedEvent>, UserRemovedEventHandler>();
            return services;
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
