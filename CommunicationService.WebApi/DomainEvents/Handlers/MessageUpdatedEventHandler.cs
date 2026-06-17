using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.DomainEvents.Handlers
{
    public class MessageUpdatedEventHandler : IDomainEventHandler<MessageUpdatedEvent>
    {
        IKafkaService _kafkaService;
        public MessageUpdatedEventHandler(IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }
        public async Task Handle(MessageUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                "MessageUpdated",
                domainEvent.Id.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}