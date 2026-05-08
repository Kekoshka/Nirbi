using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationService.WebApi.Common.AvroSchemas;
using NotificationService.WebApi.Common.ExternalApi;
using NotificationService.WebApi.Common.Hubs;
using NotificationService.WebApi.Common.Mappers;
using NotificationService.WebApi.Common.Options;
using System.Text.Json;

namespace NotificationService.WebApi.Services.BackgroundServices
{
    /// <summary>
    /// Сервис обработки уведомлений о обновления статуса платежа из Kafka
    /// </summary>
    public class PaymentUpdatedConsumer : ConsumerBase<PaymentUpdated>
    {

        PaymentMapper _paymentMapper;
        IOrderServiceApi _orderServiceApi;
        public PaymentUpdatedConsumer(IOptions<ExternalServicesOptions> externalServicesOptions,
            IOptions<KafkaConsumersOptions> kafkaConsumersOptions, 
            ISchemaRegistryClient schemaRegistryClient, 
            IHubContext<NotificationHub> hub, 
            ILogger<PaymentUpdatedConsumer> logger,
            PaymentMapper paymentMapper,
            IOrderServiceApi orderServiceApi) 
            : base(externalServicesOptions, 
                  kafkaConsumersOptions, 
                  schemaRegistryClient, 
                  hub, 
                  logger)
        {
            _paymentMapper = paymentMapper;
            _orderServiceApi = orderServiceApi;
        }

        /// <summary>
        /// Бесперерывно обрабатывает сообщения о обновлениях статуса платежа
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_externalServicesOptions.PaymentServiceTopic);

            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var message = consumeResult.Message.Value;
                    if (message is not null)
                    {
                        var payment = _paymentMapper.PaymentUpdatedToPayment(message);
                        var order = await _orderServiceApi.GetOrderAsync(message.OrderId, cancellationToken);
                        if (order is null)
                        {
                            _logger.LogError($"Payment update message was not sent because the order with id {message.OrderId} was not found.");
                            continue;
                        }
                        var jsonPayment = JsonSerializer.Serialize(payment);
                        await _hub.Clients.User(order.EmailClient).SendAsync("ShowPaymentUpdated", jsonPayment, cancellationToken);
                    }
                }
            });
         
            _consumer.Close();
        }
    }
}
