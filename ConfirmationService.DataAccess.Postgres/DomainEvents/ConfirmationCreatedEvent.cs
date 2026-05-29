using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace ConfirmationService.DataAccess.Postgres.DomainEvents
{
    public record class ConfirmationCreatedEvent(
        Guid ConfirmationId,
        string ConfirmationType,
        Guid EntityId,
        Guid InitiatorId,
        Guid ReviewerId,
        string Status,
        string MetaData,
        DateTime CreatedAt,
        DateTime ExpiresAt)
        : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

}

