using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationService.WebApi.Common.Hubs;
using NotificationService.WebApi.Common.Options;

namespace NotificationService.WebApi.Services.BackgroundServices
{
    public class ConsumerBase<TMessage> : BackgroundService
    {
        /// <summary>
        /// Hub SignalR
        /// </summary>
        protected IHubContext<NotificationHub> _hub;

        /// <summary>
        /// Слушатель сообщений о создании заказов
        /// </summary>
        protected IConsumer<string, TMessage> _consumer;

        /// <summary>
        /// Настройки слушателей Kafka
        /// </summary>
        protected KafkaConsumersOptions _kafkaConsumersOptions;

        /// <summary>
        /// Настройки внешних сервисов
        /// </summary>
        protected ExternalServicesOptions _externalServicesOptions;

        /// <summary>
        /// Клиент schema registry
        /// </summary>
        protected ISchemaRegistryClient _schemaRegistry;

        /// <summary>
        /// Логгер
        /// </summary>
        protected ILogger<ConsumerBase<TMessage>> _logger;

        public ConsumerBase(
            IOptions<ExternalServicesOptions> externalServicesOptions,
            IOptions<KafkaConsumersOptions> kafkaConsumersOptions,
            ISchemaRegistryClient schemaRegistryClient,
            IHubContext<NotificationHub> hub,
            ILogger<ConsumerBase<TMessage>> logger)
        {
            _hub = hub;
            _kafkaConsumersOptions = kafkaConsumersOptions.Value;
            _externalServicesOptions = externalServicesOptions.Value;
            _schemaRegistry = schemaRegistryClient;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _externalServicesOptions.KafkaAddress,
                GroupId = _kafkaConsumersOptions.GroupId,
                AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_kafkaConsumersOptions.AutoOffsetReset ?? "Earliest"),
                EnableAutoCommit = bool.Parse(_kafkaConsumersOptions.EnableAutoCommit ?? "true"),
            };

            _consumer = new ConsumerBuilder<string, TMessage>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(new AvroDeserializer<TMessage>(_schemaRegistry).AsSyncOverAsync<TMessage>())
                .SetErrorHandler((_, e) =>
                {
                    _logger.LogError("Kafka error: {Reason}", e.Reason);
                })
                .Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
