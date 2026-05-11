using Microsoft.Extensions.Options;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Events;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;
using MinorTaskService.Mapping;
using MinorTaskService.WebApi.Common.Options;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.DomainEvents.Handlers
{
    public class MinorTaskDeletedEventHandler : IDomainEventHandler<MinorTaskDeletedEvent>
    {
        IKafkaService _kafkaService;
        ExternalServicesOptions _externalServicesOptoins;
        public MinorTaskDeletedEventHandler(
            IKafkaService kafkaService,
            IOptions<ExternalServicesOptions> externalServicesOptoins)
        {
            _kafkaService = kafkaService;
            _externalServicesOptoins = externalServicesOptoins.Value;
        }

        public async Task Handle(MinorTaskDeletedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                _externalServicesOptoins.MinorTaskServiceTopic,
                domainEvent.MinorTaskId.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}
