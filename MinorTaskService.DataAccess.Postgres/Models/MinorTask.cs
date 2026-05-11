
using MinorTaskService.DataAccess.Postgres.DomainEvents;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Events;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace MinorTaskService.DataAccess.Postgres.Models
{
    public class MinorTask : IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int NumberVolunteers { get; set; }
        public decimal Encouragement { get; set; }
        public Guid StatusId { get; set; }
        public Guid ConsumerId { get; set; }
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Ссылка на FileCollection(содержащая только изображения) из DataService
        /// </summary>
        public Guid FileCollectionId { get; set; }
        public bool IsDeleted { get; set; }
        public Status Status { get; set; } = null!;
        public ICollection<TaskParticipant> EventParticipants { get; set; } = new List<TaskParticipant>();

        public MinorTask(
            string name,
            string description,
            decimal latitude,
            decimal longitude,
            int numberVolunteers,
            decimal encouragement,
            Guid statusId,
            Guid consumerId,
            Guid fileCollectionId)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Name = name;
            Description = description;
            Latitude = latitude;
            Longitude = longitude;
            NumberVolunteers = numberVolunteers;
            Encouragement = encouragement;
            StatusId = statusId;
            ConsumerId = consumerId;
            FileCollectionId = fileCollectionId;

            _domainEvents.Add(new MinorTaskCreatedEvent(Id, Name, Description, Latitude, Longitude, NumberVolunteers, Encouragement, StatusId, ConsumerId, CreatedAt, FileCollectionId));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();

        public void Update(string name, string description, decimal latitude, decimal longitude, int numberVolunteers, decimal encouragement)
        {
            Name = name;
            Description = description;
            Latitude = latitude;
            Longitude = longitude;
            NumberVolunteers = numberVolunteers;
            Encouragement = encouragement;

            _domainEvents.Add(new MinorTaskUpdatedEvent(Id, Name, Description, Latitude, Longitude, NumberVolunteers, Encouragement));
        }

        public void UpdateStatus(Guid statusId)
        {
            StatusId = statusId;
            _domainEvents.Add(new MinorTaskStatusUpdatedEvent(Id, StatusId));
        }
         public void Delete()
        {
            IsDeleted = true;
            _domainEvents.Add(new MinorTaskDeletedEvent(Id));
        }

    }
}
