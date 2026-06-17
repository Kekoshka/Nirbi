namespace CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces
{
    public interface IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
    }
}
