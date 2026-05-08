namespace NotificationService.WebApi.Common.Events
{
    public record MinorTaskDeletedEvent(
        Guid MinorTaskId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
