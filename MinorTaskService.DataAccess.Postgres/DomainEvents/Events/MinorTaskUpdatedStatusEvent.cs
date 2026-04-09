using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.DomainEvents.Events
{
    public record MinorTaskStatusUpdatedEvent(
        Guid MinorTaskId,
        Guid StatusId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public class MinorTaskStatusUpdatedEventHandler : IDomainEventHandler<MinorTaskStatusUpdatedEvent>
    {
        public Task Handle(MinorTaskStatusUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
