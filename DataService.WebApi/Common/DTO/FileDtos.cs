namespace DataService.WebApi.Common.DTO;

public record FileMetadataDto(
    Guid Id,
    Guid? FileCollectionId,
    int SortOrder,
    string ContentType,
    long SizeBytes,
    string? OriginalFileName,
    DateTime CreatedAtUtc);

public record class CollectionPreviewRequest(
    List<Guid> CollectionIds);

public record class CollectionPreviewDto(
    Guid CollectionId,
    Guid FileId,
    string? ContentType,
    string Data = "");
