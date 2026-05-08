namespace NotificationService.WebApi.Common.Events
{
    public record MinorTaskUpdatedEvent(
        Guid MinorTaskId,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
