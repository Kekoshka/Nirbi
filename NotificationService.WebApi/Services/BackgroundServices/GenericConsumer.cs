using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationService.WebApi.Common.Hubs;
using NotificationService.WebApi.Common.Options;
using NotificationService.WebApi.Services.BackgroundServices;
using System.Text.Json;

namespace NotificationService.WebApi.Services.BackgroundServices;

public class GenericConsumer<TAvroMessage> : ConsumerBase<TAvroMessage>
    where TAvroMessage : class, ISpecificRecord
{
    private readonly Func<TAvroMessage, List<string>?> _recipientResolver;
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
        Func<TAvroMessage, List<string>?> recipientResolver)
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
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var message = consumeResult.Message.Value;

                    if (message is null) continue;

                    var recipientId = _recipientResolver(message);

                    if (recipientId is null)
                    {
                        _logger.LogError(
                            "Notification [{HubMethod}] was not sent: recipient not found.",
                            _hubMethod);
                        continue;
                    }

                    var json = JsonSerializer.Serialize(_eventMapper(message));
                    await _hub.Clients
                        .Users(recipientId)
                        .SendAsync(_hubMethod, json, cancellationToken);
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    _logger.LogWarning(
                        "Topic not available yet for [{HubMethod}], retrying in 5s...",
                        _hubMethod);

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in consumer [{HubMethod}]", _hubMethod);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }, cancellationToken);

        _consumer.Close();
    }
}