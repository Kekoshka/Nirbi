using Confluent.SchemaRegistry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CommunicationService.DataAccess.Postgres.Context;
using CommunicationService.WebApi.Common.Options;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.Services.BackgroundServices
{
    public class TaskParticipantRemovedConsumer : ConsumerBase<TaskParticipantRemoved>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TaskParticipantRemovedConsumer(
            IOptions<ExternalServicesOptions> externalServicesOptions,
            IOptions<KafkaConsumersOptions> kafkaConsumersOptions,
            ISchemaRegistryClient schemaRegistryClient,
            ILogger<TaskParticipantRemovedConsumer> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(externalServicesOptions, kafkaConsumersOptions, schemaRegistryClient, logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe("TaskParticipantRemoved");

            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);
                        var message = consumeResult.Message.Value;
                        if (message is null) continue;

                        var minorTaskId = Guid.Parse(message.MinorTaskId);
                        var userId = Guid.Parse(message.UserId);

                        using var scope = _serviceScopeFactory.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var chatUserService = scope.ServiceProvider.GetRequiredService<IChatUserService>();

                        var chat = await context.Chats
                            .Include(c => c.ChatUsers)
                            .FirstOrDefaultAsync(
                                c => c.Name == minorTaskId.ToString(),
                                cancellationToken);

                        if (chat is null)
                        {
                            _logger.LogWarning(
                                "Chat for MinorTask {MinorTaskId} not found", minorTaskId);
                            continue;
                        }

                        // Пользователь уже не состоит в чате — пропускаем
                        bool isMember = chat.ChatUsers
                            .Any(cu => cu.UserId == userId && !cu.IsDeleted);
                        if (!isMember) continue;

                        await chatUserService.RemoveChatUserAsync(chat.Id, userId, cancellationToken);

                        _logger.LogInformation(
                            "User {UserId} removed from chat {ChatId} (task {MinorTaskId})",
                            userId, chat.Id, minorTaskId);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing TaskParticipantRemoved");
                    }
                }
            }, cancellationToken);

            _consumer.Close();
        }
    }
}