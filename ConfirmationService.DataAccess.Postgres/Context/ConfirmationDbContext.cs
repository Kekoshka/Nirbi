using ConfirmationService.DataAccess.Models;
using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConfirmationService.DataAccess.Context;

public class ConfirmationDbContext : DbContext
{
    IDomainEventDispatcher _dispatcher;
    public ConfirmationDbContext(DbContextOptions<ConfirmationDbContext> options, IDomainEventDispatcher dispatcher) : base(options)
    {
        Database.EnsureCreated();
        _dispatcher = dispatcher;

    }

    public DbSet<Confirmation> Confirmations { get; set; }
    public DbSet<ConfirmationAudit> ConfirmationAudits { get; set; }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _dispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        return result;
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }

}