using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.DomainEvents.Handlers
{
    public class MessageDeletedEventHandler : IDomainEventHandler<MessageDeletedEvent>
    {
        IKafkaService _kafkaService;
        public MessageDeletedEventHandler(IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }
        public async Task Handle(MessageDeletedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                "MessageDeleted",
                domainEvent.Id.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}