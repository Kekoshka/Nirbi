using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;

namespace CommunicationService.WebApi.DomainEvents.Handlers
{
    public class UserJoinedEventHandler : IDomainEventHandler<UserJoinedEvent>
    {
        IKafkaService _kafkaService;
        public UserJoinedEventHandler(IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }


        public async Task Handle(UserJoinedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                "UserJoined",
                domainEvent.UserId.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}
