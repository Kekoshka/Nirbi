using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;
using ConfirmationService.WebApi.Common.Options;
using ConfirmationService.WebApi.DomainEvents.Events;
using ConfirmationService.WebApi.Interfaces;
using Microsoft.Extensions.Options;

namespace ConfirmationService.WebApi.DomainEvents
{
    public class ConfirmationRespondEventHandler : IDomainEventHandler<ConfirmationRespondEvent>
    {
        IKafkaService _kafkaService;
        ExternalServicesOptions _externalServicesOptoins;
        public ConfirmationRespondEventHandler(
            IKafkaService kafkaService,
            IOptions<ExternalServicesOptions> externalServicesOptoins)
        {
            _kafkaService = kafkaService;
            _externalServicesOptoins = externalServicesOptoins.Value;
        }


        public async Task Handle(ConfirmationRespondEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                _externalServicesOptoins.ConfirmationServiceTopic,
                domainEvent.ConfirmationId,
                domainEvent,
                cancellationToken);
        }
    }
}
