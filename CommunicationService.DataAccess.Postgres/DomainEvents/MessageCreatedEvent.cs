using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace CommunicationService.DataAccess.Postgres.DomainEvents
{
    public record class MessageCreatedEvent(
        Guid Id,
        Guid Sender,
        Guid ChatId,
        string Content,
        DateTime CreatedAt,
        List<Guid> ChatUsers) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    };
}
