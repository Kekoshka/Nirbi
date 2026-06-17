using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.DomainEvents.Handlers
{
    public class MessageCreatedEventHandler : IDomainEventHandler<MessageCreatedEvent>
    {
        IKafkaService _kafkaService;
        public MessageCreatedEventHandler(IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }
        public async Task Handle(MessageCreatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                "MessageCreated",
                domainEvent.Id.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}