using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.DomainEvents.Handlers
{
    public class ChatCreatedEventHandler : IDomainEventHandler<ChatCreatedEvent>
    {
        IKafkaService _kafkaService;
        public ChatCreatedEventHandler(IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }
        public async Task Handle(ChatCreatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                "ChatCreated",
                domainEvent.Id.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}