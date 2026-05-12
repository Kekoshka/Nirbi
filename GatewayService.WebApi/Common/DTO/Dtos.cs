namespace GatewayService.WebApi.Common.DTO;

// ─── MinorTask ───────────────────────────────────────────────────────────────

public class CreateMinorTaskGatewayRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int NumberVolunteers { get; set; }
    public double Encouragement { get; set; }
    /// <summary>Файлы изображений (опционально)</summary>
    public List<IFormFile>? Images { get; set; }
}

public class MinorTaskResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int NumberVolunteers { get; set; }
    public double Encouragement { get; set; }
    public Guid? FileCollectionId { get; set; }
}

/// <summary>Ответ со списком задач — первое изображение вместо коллекции</summary>
public class MinorTaskListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int NumberVolunteers { get; set; }
    public double Encouragement { get; set; }
    /// <summary>URL первого изображения из коллекции</summary>
    public string? PreviewImageUrl { get; set; }
}

/// <summary>Ответ для одной задачи — полный список изображений</summary>
public class MinorTaskDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int NumberVolunteers { get; set; }
    public double Encouragement { get; set; }
    public List<FileMetadataDto> Images { get; set; } = [];
}

// ─── DataService ─────────────────────────────────────────────────────────────

public class FileMetadataDto
{
    public Guid Id { get; set; }
    public Guid? FileCollectionId { get; set; }
    public int SortOrder { get; set; }
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    /// <summary>Ссылка для скачивания (добавляется gateway'ем)</summary>
    public string? DownloadUrl { get; set; }
}
