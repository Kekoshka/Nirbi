namespace NotificationService.WebApi.Common.Events
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

