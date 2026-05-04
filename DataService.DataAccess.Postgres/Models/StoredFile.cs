namespace DataService.DataAccess.Postgres.Models;

public class StoredFile
{
    public Guid Id { get; set; }

    /// <summary>When set, the file belongs to a collection; otherwise it is standalone.</summary>
    public Guid? FileCollectionId { get; set; }

    public FileCollection? FileCollection { get; set; }

    public int SortOrder { get; set; }

    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
