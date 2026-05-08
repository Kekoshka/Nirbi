public class GenericConsumer<TAvroMessage> : ConsumerBase<TAvroMessage>
    where TAvroMessage : class, ISpecificRecord
{
    private readonly Func<TAvroMessage, CancellationToken, Task<string?>> _recipientResolver;
    private readonly Func<TAvroMessage, object> _eventMapper;
    private readonly string _hubMethod;
    private readonly string _topic;

    public GenericConsumer(
        IOptions<ExternalServicesOptions> externalServicesOptions,
        IOptions<KafkaConsumersOptions> kafkaConsumersOptions,
        ISchemaRegistryClient schemaRegistryClient,
        IHubContext<NotificationHub> hub,
        ILogger<GenericConsumer<TAvroMessage>> logger,
        string topic,
        string hubMethod,
        Func<TAvroMessage, object> eventMapper,
        Func<TAvroMessage, CancellationToken, Task<string?>> recipientResolver)
        : base(externalServicesOptions, kafkaConsumersOptions, schemaRegistryClient, hub, logger)
    {
        _topic = topic;
        _hubMethod = hubMethod;
        _eventMapper = eventMapper;
        _recipientResolver = recipientResolver;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);

        await Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                var message = consumeResult.Message.Value;

                if (message is null) continue;

                var recipientId = await _recipientResolver(message, cancellationToken);

                if (recipientId is null)
                {
                    _logger.LogError(
                        "Notification [{HubMethod}] was not sent: recipient not found. Message: {Message}",
                        _hubMethod, message);
                    continue;
                }

                var json = JsonSerializer.Serialize(_eventMapper(message));

                await _hub.Clients
                    .User(recipientId)
                    .SendAsync(_hubMethod, json, cancellationToken);
            }
        }, cancellationToken);

        _consumer.Close();
    }
}