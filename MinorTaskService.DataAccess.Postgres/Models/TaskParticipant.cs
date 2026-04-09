using MinorTaskService.DataAccess.Postgres.DomainEvents;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.Models
{
    public class TaskParticipant
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public Guid MinorTaskId { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
        public MinorTask MinorTask { get; set; }

        public TaskParticipant(Guid minorTaskId, Guid userId)
        {
            MinorTaskId = minorTaskId;
            UserId = userId;
            IsActive = true;
            _domainEvents.Add(new TaskParticipantAddedEvent(MinorTaskId, UserId));
        }
        
        public void Remove()
        {
            IsActive = false;
            _domainEvents.Add(new TaskParticipantRemovedEvent(MinorTaskId, UserId));
        }
    }
}
