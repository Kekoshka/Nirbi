namespace NotificationService.WebApi.Common.Events
{
    public record class MessageUpdatedEvent(
        Guid Id,
        Guid ChatId,
        string Content,
        List<Guid> ChatUsers) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
