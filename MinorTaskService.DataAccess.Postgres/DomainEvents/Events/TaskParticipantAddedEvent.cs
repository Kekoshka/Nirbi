using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.DomainEvents.Events
{
    public record TaskParticipantAddedEvent(
        Guid MinorTaskId,
        Guid UserId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public class TaskParticipantAddedEventHandler : IDomainEventHandler<TaskParticipantAddedEvent>
    {
        public Task Handle(TaskParticipantAddedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
