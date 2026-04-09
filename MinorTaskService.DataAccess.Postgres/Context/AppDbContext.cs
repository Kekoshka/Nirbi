using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Configurations;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;
using MinorTaskService.DataAccess.Postgres.Models;

namespace MinorTaskService.DataAccess.Postgres.Context
{
    public class AppDbContext : DbContext
    {
        IDomainEventDispatcher _dispatcher;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MinorTask> MinorTasks { get; set; }
        public DbSet<TaskParticipant> TaskParticipants { get; set; }
        public DbSet<Status> Statuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddFilters();
            modelBuilder.ConfigureTaskParticipants();
        }

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
