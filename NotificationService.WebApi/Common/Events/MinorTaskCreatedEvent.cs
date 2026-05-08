namespace NotificationService.WebApi.Common.Events
{
    public record MinorTaskCreatedEvent(
        Guid MinorTaskId,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        Guid StatusId,
        Guid ConsumerId,
        DateTime CreatedAt) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
