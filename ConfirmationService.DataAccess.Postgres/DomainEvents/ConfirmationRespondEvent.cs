using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace ConfirmationService.DataAccess.Postgres.DomainEvents
{
    public record class ConfirmationRespondEvent(
        Guid ConfirmationId,
        string ConfirmationType,
        Guid InitiatorId,
        Guid ReviewerId,
        Guid EntityId,
        bool IsAccepted) 
        : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}

