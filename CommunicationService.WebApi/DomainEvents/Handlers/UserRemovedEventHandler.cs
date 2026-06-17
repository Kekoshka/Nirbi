using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.DomainEvents.Handlers
{
    public class UserRemovedEventHandler : IDomainEventHandler<UserRemovedEvent>
    {
        IKafkaService _kafkaService;
        public UserRemovedEventHandler(IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }
        public async Task Handle(UserRemovedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                "UserRemoved",
                domainEvent.UserId.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}