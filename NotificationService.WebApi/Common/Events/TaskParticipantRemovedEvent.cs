namespace NotificationService.WebApi.Common.Events
{
    public record TaskParticipantRemovedEvent(
        Guid MinorTaskId,
        Guid UserId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
