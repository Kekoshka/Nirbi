namespace NotificationService.WebApi.Common.Events
{
    public record MinorTaskStatusUpdatedEvent(
        Guid MinorTaskId,
        Guid StatusId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
