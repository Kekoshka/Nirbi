using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationService.Mapping;
using NotificationService.WebApi.Common.AvroSchemas;
using NotificationService.WebApi.Common.Hubs;
using NotificationService.WebApi.Common.Options;
using NotificationService.WebApi.Interfaces;

namespace NotificationService.WebApi.Services.BackgroundServices;
public static class KafkaConsumersRegistration
{
    public static IServiceCollection AddKafkaConsumers(
        this IServiceCollection services,
        ExternalServicesOptions options)
    {
        services.AddHostedService(sp => new GenericConsumer<ConfirmationCreated>(
            topic: "ConfirmationCreated",
            hubMethod: "ShowConfirmationCreated",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ReviewerId,
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ConfirmationCreated>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<ConfirmationRespond>(
            topic: "ConfirmationRespond",
            hubMethod: "ShowConfirmationRespond",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.InitiatorId,
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ConfirmationRespond>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<ConfirmationRevoked>(
            topic: "ConfirmationRevoked",
            hubMethod: "ShowConfirmationRevoked",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ReviewerId,
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ConfirmationRevoked>>>()
        ));

        return services;
    }
}