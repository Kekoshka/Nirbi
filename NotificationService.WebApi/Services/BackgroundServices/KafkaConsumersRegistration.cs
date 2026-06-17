using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationService;
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
            recipientResolver: (msg) => new List<string>() { msg.ReviewerId },
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
            recipientResolver: (msg) => new List<string>() { msg.InitiatorId },
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
            recipientResolver: (msg) => new List<string>() { msg.ReviewerId },
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ConfirmationRevoked>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<ChatCreated>(
            topic: "ChatCreated",
            hubMethod: "ChatCreated",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ChatUsers.Select(cu => cu.ToString()).ToList(),
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ChatCreated>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<MessageCreated>(
            topic: "MessageCreated",
            hubMethod: "MessageCreated",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ChatUsers.Select(cu => cu.ToString()).ToList(),
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<MessageCreated>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<MessageDeleted>(
            topic: "MessageDeleted",
            hubMethod: "MessageDeleted",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ChatUsers.Select(cu => cu.ToString()).ToList(),
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<MessageDeleted>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<MessageUpdated>(
            topic: "MessageUpdated",
            hubMethod: "MessageUpdated",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ChatUsers.Select(cu => cu.ToString()).ToList(),
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<MessageUpdated>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<UserJoined>(
            topic: "UserJoined",
            hubMethod: "UserJoined",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ChatUsers.Select(cu => cu.ToString()).ToList(),
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<UserJoined>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<UserRemoved>(
            topic: "UserRemoved",
            hubMethod: "UserRemoved",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: (msg) => msg.ChatUsers.Select(cu => cu.ToString()).ToList(),
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<UserRemoved>>>()
        ));



        return services;
    }
}