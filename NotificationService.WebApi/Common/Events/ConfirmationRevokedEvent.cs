namespace NotificationService.WebApi.Common.Events;
public record class ConfirmationRevokedEvent(Guid ConfirmationId, Guid ReviewerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
