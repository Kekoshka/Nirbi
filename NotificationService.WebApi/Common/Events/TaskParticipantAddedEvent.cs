namespace NotificationService.WebApi.Common.Events
{
    public record TaskParticipantAddedEvent(
        Guid MinorTaskId,
        Guid UserId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
