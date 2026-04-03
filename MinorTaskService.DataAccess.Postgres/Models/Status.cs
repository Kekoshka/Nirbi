namespace MinorTaskService.DataAccess.Postgres.Models
{
    public class Status
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<MinorTask> MinorTasks { get; set; }
    }
}
