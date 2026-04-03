using NetTopologySuite.Geometries;

namespace MinorTaskService.DataAccess.Postgres.Models
{
    public class MinorTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Point Place { get; set; }
        public int NumberVolunteers { get; set; }
        public decimal Encouragement { get; set; }
        public Guid StatusId { get; set; }
        public Guid ChatId { get; set; }
        public Guid Images { get; set; }
        public Guid Consumer { get; set; }
        public DateTime CreatedAt { get; set; }
        public Status Status { get; set; }
        public ICollection<TaskParticipant> EventParticipants { get; set; }
    }
}
