using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.DomainEvents.Events
{
    public record MinorTaskUpdatedEvent(
        Guid MinorTaskId,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public class MinorTaskUpdatedEventHandler : IDomainEventHandler<MinorTaskUpdatedEvent>
    {
        public Task Handle(MinorTaskUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
