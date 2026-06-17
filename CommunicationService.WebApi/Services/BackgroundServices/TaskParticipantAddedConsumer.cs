using Confluent.SchemaRegistry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CommunicationService.DataAccess.Postgres.Context;
using CommunicationService.WebApi.Common.Options;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.Services.BackgroundServices
{
    public class TaskParticipantAddedConsumer : ConsumerBase<TaskParticipantAdded>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TaskParticipantAddedConsumer(
            IOptions<ExternalServicesOptions> externalServicesOptions,
            IOptions<KafkaConsumersOptions> kafkaConsumersOptions,
            ISchemaRegistryClient schemaRegistryClient,
            ILogger<TaskParticipantAddedConsumer> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(externalServicesOptions, kafkaConsumersOptions, schemaRegistryClient, logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe("TaskParticipantAdded");

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

                        // Ищем групповой чат, привязанный к задаче.
                        // Соглашение: Name чата совпадает с MinorTaskId (устанавливается
                        // при создании задачи в ChatCreatedEvent).
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

                        // Пользователь уже в чате — пропускаем
                        bool alreadyMember = chat.ChatUsers
                            .Any(cu => cu.UserId == userId && !cu.IsDeleted);
                        if (alreadyMember) continue;

                        await chatUserService.JoinChatUserAsync(chat.Id, userId, cancellationToken);

                        _logger.LogInformation(
                            "User {UserId} added to chat {ChatId} (task {MinorTaskId})",
                            userId, chat.Id, minorTaskId);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing TaskParticipantAdded");
                    }
                }
            }, cancellationToken);

            _consumer.Close();
        }
    }
}