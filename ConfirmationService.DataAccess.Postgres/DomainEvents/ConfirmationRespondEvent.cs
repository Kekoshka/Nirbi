using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace ConfirmationService.WebApi.DomainEvents.Events
{
    public record class ConfirmationRespondEvent(
        Guid ConfirmationId,
        Guid InitiatorId,
        bool IsAccepted) 
        : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}

