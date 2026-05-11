using ConfirmationService.DataAccess.Postgres.DomainEvents;
using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;
using ConfirmationService.Mapping;
using ConfirmationService.WebApi.Common.Options;
using ConfirmationService.WebApi.Interfaces;
using Microsoft.Extensions.Options;

namespace ConfirmationService.WebApi.DomainEvents
{
    public class ConfirmationRevokedEventHandler : IDomainEventHandler<ConfirmationRevokedEvent>
    {
        IKafkaService _kafkaService;
        ExternalServicesOptions _externalServicesOptoins;
        public ConfirmationRevokedEventHandler(
            IKafkaService kafkaService,
            IOptions<ExternalServicesOptions> externalServicesOptoins)
        {
            _kafkaService = kafkaService;
            _externalServicesOptoins = externalServicesOptoins.Value;
        }


        public async Task Handle(ConfirmationRevokedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _kafkaService.ProduceAsync(
                _externalServicesOptoins.ConfirmationServiceTopic,
                domainEvent.ConfirmationId.ToString(),
                domainEvent.ToAvro(),
                cancellationToken);
        }
    }
}
