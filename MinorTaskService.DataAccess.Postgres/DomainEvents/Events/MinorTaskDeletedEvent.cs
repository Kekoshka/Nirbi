using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.DomainEvents.Events
{
    public record MinorTaskDeletedEvent(
        Guid MinorTaskId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public class MinorTaskDeletedEventHandler : IDomainEventHandler<MinorTaskDeletedEvent>
    {
        public Task Handle(MinorTaskDeletedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
