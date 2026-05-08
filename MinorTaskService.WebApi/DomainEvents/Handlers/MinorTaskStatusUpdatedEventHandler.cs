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
        ExternalServicesOptions _externalServicesOptoins;
        public MinorTaskStatusUpdatedEventHandler(
            IKafkaService kafkaService,
            IOptions<ExternalServicesOptions> externalServicesOptoins)
        {
            _kafkaService = kafkaService;
            _externalServicesOptoins = externalServicesOptoins.Value;
        }

        public async Task Handle(MinorTaskStatusUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                _externalServicesOptoins.MinorTaskServiceTopic,
                domainEvent.MinorTaskId,
                domainEvent.ToAvro,
                cancellationToken);
        }
    }
}
