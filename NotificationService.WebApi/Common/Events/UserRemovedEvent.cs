namespace NotificationService.WebApi.Common.Events
{
    public record class UserRemovedEvent(
        Guid UserId,
        Guid ChatId,
        List<Guid> ChatUsers) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
