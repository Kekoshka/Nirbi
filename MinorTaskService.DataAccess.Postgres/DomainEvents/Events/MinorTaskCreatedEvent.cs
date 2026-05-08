using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.DomainEvents.Events
{
    public record MinorTaskCreatedEvent(
        Guid MinorTaskId,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        Guid StatusId,
        Guid ConsumerId,
        DateTime CreatedAt,
        Guid FileCollectionId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
