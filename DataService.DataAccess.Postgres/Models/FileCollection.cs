namespace DataService.DataAccess.Postgres.Models;

public class FileCollection
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<StoredFile> Files { get; set; } = new List<StoredFile>();
}
