using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Options;
using MinorTaskService;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.Options;
using System.Text.Json;

namespace MinorTaskService.WebApi.Services.BackgroundServices
{
    /// <summary>
    /// Обрабатывает сообщения о создании заказов
    /// </summary>
    public class ConfirmationRespondConsumer : ConsumerBase<ConfirmationRespond>
    {
        IServiceScopeFactory _serviceScopeFactory;
        public ConfirmationRespondConsumer(IOptions<ExternalServicesOptions> externalServicesOptions,
            IOptions<KafkaConsumersOptions> kafkaConsumersOptions,
            ISchemaRegistryClient schemaRegistryClient, 
            ILogger<ConfirmationRespondConsumer> logger,
            IServiceScopeFactory serviceScopeFactory) 
            : base(externalServicesOptions, 
                  kafkaConsumersOptions, 
                  schemaRegistryClient, 
                  logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Выполняет непрерывную обработку сообщений о создании заказов из Kafka 
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _consumer.Subscribe("ConfirmationRespond");

            await Task.Run(async ()  =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);
                        var message = consumeResult.Message.Value;
                        if (message is not null)
                        {
                            if (!message.IsAccepted)
                                return;
                            var confirmationType = message.ConfirmationType;
                            var initiatorId = Guid.Parse(message.InitiatorId);
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
                            if (confirmationType == "Invite to task")
                            {
                                TaskParticipant newTaskParticipant = new(minorTaskId, reviewerId);
                                minorTask.EventParticipants.Add(newTaskParticipant);
                            }
                            if (confirmationType == "Respond to minor task")
                            {
                                TaskParticipant newTaskParticipant = new(minorTaskId, initiatorId);
                                minorTask.EventParticipants.Add(newTaskParticipant);
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    
                }
            });

            _consumer.Close();
        }
    }
}
