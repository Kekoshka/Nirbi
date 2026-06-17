using Microsoft.Extensions.Options;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Events;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;
using MinorTaskService.Mapping;
using MinorTaskService.WebApi.Common.Options;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.DomainEvents.Handlers
{
    public class MinorTaskStatusUpdatedEventHandler : IDomainEventHandler<MinorTaskStatusUpdatedEvent>
    {
        IKafkaService _kafkaService;
        public MinorTaskStatusUpdatedEventHandler(
            IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }

        public async Task Handle(MinorTaskStatusUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                "MinorTaskStatusUpdated",
                domainEvent.MinorTaskId.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}
