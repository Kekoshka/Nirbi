namespace DataService.WebApi.Common.DTO;

public record FileMetadataDto(
    Guid Id,
    Guid? FileCollectionId,
    int SortOrder,
    string ContentType,
    long SizeBytes,
    string? OriginalFileName,
    DateTime CreatedAtUtc);
