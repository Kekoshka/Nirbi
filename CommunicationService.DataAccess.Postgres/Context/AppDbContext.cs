using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using CommunicationService.DataAccess.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.DataAccess.Postgres.Context
{
    public class AppDbContext : DbContext
    {
        IDomainEventDispatcher _dispatcher;
        public AppDbContext(
            DbContextOptions<AppDbContext> options, 
            IDomainEventDispatcher dispatcher) 
            : base (options)
        {
            _dispatcher = dispatcher;
            Database.EnsureCreated();
        }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatType> ChatTypes { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
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
}
