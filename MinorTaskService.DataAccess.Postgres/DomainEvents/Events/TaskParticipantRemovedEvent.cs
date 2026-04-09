using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.DomainEvents.Events
{
    public record TaskParticipantRemovedEvent(
        Guid MinorTaskId,
        Guid UserId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public class TaskParticipantRemovedEventHandler : IDomainEventHandler<TaskParticipantRemovedEvent>
    {
        public Task Handle(TaskParticipantRemovedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
