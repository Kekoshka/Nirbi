namespace ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }

}
