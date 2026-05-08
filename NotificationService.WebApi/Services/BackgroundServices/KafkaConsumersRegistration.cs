using NotificationService.Mapping;
using NotificationService.WebApi.Common.AvroSchemas;
using NotificationService.WebApi.Common.Options;

public static class KafkaConsumersRegistration
{
    public static IServiceCollection AddKafkaConsumers(
        this IServiceCollection services,
        ExternalServicesOptions options)
    {
        // ── ConfirmationService ────────────────────────────────────────────

        services.AddHostedService(sp => new GenericConsumer<ConfirmationCreated>(
            topic: options.ConfirmationCreatedTopic,
            hubMethod: "ShowConfirmationCreated",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: async (msg, ct) =>
            {
                return msg.ReviewerId
            },
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ConfirmationCreated>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<ConfirmationRespond>(
            topic: options.ConfirmationRespondTopic,
            hubMethod: "ShowConfirmationRespond",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: async (msg, ct) =>
            {
                var user = await sp.GetRequiredService<IUserServiceApi>()
                    .GetUserAsync(Guid.Parse(msg.InitiatorId), ct);
                return user?.Email;
            },
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ConfirmationRespond>>>()
        ));

        services.AddHostedService(sp => new GenericConsumer<ConfirmationRevoked>(
            topic: options.ConfirmationRevokedTopic,
            hubMethod: "ShowConfirmationRevoked",
            eventMapper: msg => msg.ToEvent(),
            recipientResolver: async (msg, ct) =>
            {
                var user = await sp.GetRequiredService<IUserServiceApi>()
                    .GetUserAsync(Guid.Parse(msg.ReviewerId), ct);
                return user?.Email;
            },
            externalServicesOptions: sp.GetRequiredService<IOptions<ExternalServicesOptions>>(),
            kafkaConsumersOptions: sp.GetRequiredService<IOptions<KafkaConsumersOptions>>(),
            schemaRegistryClient: sp.GetRequiredService<ISchemaRegistryClient>(),
            hub: sp.GetRequiredService<IHubContext<NotificationHub>>(),
            logger: sp.GetRequiredService<ILogger<GenericConsumer<ConfirmationRevoked>>>()
        ));

        return services;
    }
}