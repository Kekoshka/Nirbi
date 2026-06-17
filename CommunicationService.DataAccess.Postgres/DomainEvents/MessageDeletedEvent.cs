using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace CommunicationService.DataAccess.Postgres.DomainEvents
{
    public record class MessageDeletedEvent(
        Guid Id,
        Guid ChatId,
        List<Guid> ChatUsers): IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    };
}
