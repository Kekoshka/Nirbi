namespace CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces
{
    internal interface IHasDomainEvents
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();

    }
}
