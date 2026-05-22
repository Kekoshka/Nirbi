namespace GatewayService.WebApi.Common.DTO;

// ─── MinorTask ───────────────────────────────────────────────────────────────

/// <summary>
/// Внутренний proxy-DTO для ответов из MinorTaskService.
/// Поля Status/ConsumerId — для определения статуса задачи и её владельца.
/// </summary>
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

    /// <summary>Статус задачи (Open / InProgress / Done и т.п.)</summary>
    public string? Status { get; set; }

    /// <summary>UUID пользователя-владельца задачи</summary>
    public string? ConsumerId { get; set; }
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

    /// <summary>Байты первого изображения в base64</summary>
    public string? PreviewImageData { get; set; }

    /// <summary>MIME-тип: image/jpeg, image/png и т.д.</summary>
    public string? PreviewImageContentType { get; set; }

    /// <summary>Статус задачи (Open / InProgress / Done и т.п.)</summary>
    public string? Status { get; set; }

    /// <summary>UUID пользователя-владельца — нужно фронту для "Мои задачи"</summary>
    public string? ConsumerId { get; set; }
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

    /// <summary>Статус задачи (Open / InProgress / Done и т.п.)</summary>
    public string? Status { get; set; }

    /// <summary>UUID пользователя-владельца — нужно фронту для кнопок Редактировать/Удалить</summary>
    public string? ConsumerId { get; set; }
}

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

/// <summary>Обновление полей задачи (без статуса).</summary>
public class UpdateMinorTaskGatewayRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int NumberVolunteers { get; set; }
    public double Encouragement { get; set; }
}

/// <summary>Смена статуса задачи.</summary>
public class UpdateMinorTaskStatusGatewayRequest
{
    public Guid StatusId { get; set; }
}

// ─── Confirmations Aggregation ────────────────────────────────────────────────

/// <summary>Внутренний proxy-DTO для ответов из ConfirmationService.</summary>
public class ConfirmationBaseResponse
{
    public Guid Id { get; set; }
    public string ConfirmationType { get; set; }
    public Guid EntityId { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid ReviewerId { get; set; }
    public string Status { get; set; }
    public string MetaData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? RejectionReason { get; set; }
    public ICollection<ConfirmationAuditDTO> Audits { get; set; } = new List<ConfirmationAuditDTO>();
}

public class ConfirmationAuditDTO()
{
    public Guid Id { get; set; }
    public Guid ConfirmationId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public Guid ChangedBy { get; set; }
}
/// <summary>Обогащённый ответ с username инициатора/reviewer и названием задачи.</summary>
public class EnrichedConfirmationResponse
{
    public Guid Id { get; set; }
    public string ConfirmationType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }
    /// <summary>Название задачи (или другой сущности) — обогащается Gateway.</summary>
    public string? EntityName { get; set; }

    public Guid InitiatorId { get; set; }
    /// <summary>Username инициатора — обогащается Gateway из AuthService.</summary>
    public string? InitiatorUsername { get; set; }

    public Guid ReviewerId { get; set; }
    /// <summary>Username reviewer'а — обогащается Gateway из AuthService.</summary>
    public string? ReviewerUsername { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string MetaData { get; set; }
}

// ─── Auth ─────────────────────────────────────────────────────────────────────

/// <summary>Ответ AuthService GET /api/Users/{id} — для внутреннего использования в агрегаторе.</summary>
public class UserLookupDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}

// ─── Tasks ────────────────────────────────────────────────────────────────────

/// <summary>Ответ MinorTaskService POST /api/tasks/names — для внутреннего использования.</summary>
public class TaskNameLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class FileMetadataDto
{
    public Guid Id { get; set; }
    public Guid? FileCollectionId { get; set; }
    public int SortOrder { get; set; }
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Байты файла в base64</summary>
    public string? Data { get; set; }
}
