namespace MinorTaskService.DataAccess.Postgres.Models
{
    public class TaskParticipant
    {
        public Guid MinorEventId { get; set; }
        public Guid UserId { get; set; }
        public MinorTask MinorTask { get; set; }

    }
}
