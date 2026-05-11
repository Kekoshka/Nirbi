using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinorTaskService;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.Options;
using System.Text.Json;

namespace NotificationService.WebApi.Services.BackgroundServices
{
    /// <summary>
    /// Обрабатывает сообщения о создании заказов
    /// </summary>
    public class ConfirmationRespondConsumer : ConsumerBase<ConfirmationRespond>
    {
        AppDbContext _context;
        public ConfirmationRespondConsumer(IOptions<ExternalServicesOptions> externalServicesOptions,
            IOptions<KafkaConsumersOptions> kafkaConsumersOptions,
            ISchemaRegistryClient schemaRegistryClient, 
            ILogger<ConfirmationRespondConsumer> logger,
            AppDbContext context) 
            : base(externalServicesOptions, 
                  kafkaConsumersOptions, 
                  schemaRegistryClient, 
                  logger)
        {
            _context = context;
        }

        /// <summary>
        /// Выполняет непрерывную обработку сообщений о создании заказов из Kafka 
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_externalServicesOptions.ConfirmationServiceTopic);

            await Task.Run(async ()  =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var message = consumeResult.Message.Value;
                    if (message is not null)
                    {
                        if (!message.IsAccepted)
                            return;
                        var reviewerId = Guid.Parse(message.ReviewerId);
                        var minorTaskId = Guid.Parse(message.EntityId);
                        var minorTask = await _context.MinorTasks
                        .Include(mt => mt.EventParticipants)
                        .FirstOrDefaultAsync(mt => mt.Id == minorTaskId);
                        if (minorTask is null)
                        {
                            _logger.LogError("Minor task with id {minorTaskId} not found", minorTaskId);
                            return;
                        }
                        TaskParticipant newTaskParticipant = new(minorTaskId, reviewerId);
                        minorTask.EventParticipants.Add(newTaskParticipant);
                    }
                }
            });

            _consumer.Close();
        }
    }
}
