using Microsoft.Extensions.Options;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Events;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;
using MinorTaskService.WebApi.Common.Options;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.DomainEvents.Handlers
{
    public class TaskParticipantAddedEventHandler : IDomainEventHandler<TaskParticipantAddedEvent>
    {
        IKafkaService _kafkaService;
        ExternalServicesOptions _externalServicesOptoins;
        public TaskParticipantAddedEventHandler(
            IKafkaService kafkaService,
            IOptions<ExternalServicesOptions> externalServicesOptoins)
        {
            _kafkaService = kafkaService;
            _externalServicesOptoins = externalServicesOptoins.Value;
        }
        public async Task Handle(TaskParticipantAddedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                _externalServicesOptoins.TaskParticipantServiceTopic,
                domainEvent.MinorTaskId,
                domainEvent,
                cancellationToken);
        }
    }
}
