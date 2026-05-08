namespace NotificationService.WebApi.Common.Events
{
    public interface IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
    }
}
