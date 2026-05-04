using DataService.WebApi.Common.DTO;

namespace DataService.WebApi.Interfaces;

public interface IDataObjectService
{
    Task<Guid> CreateCollectionAsync(CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(Guid collectionId, CancellationToken cancellationToken = default);

    Task<Guid> UploadStandaloneAsync(
        Stream content,
        string contentType,
        string? originalFileName,
        long? knownSizeBytes,
        CancellationToken cancellationToken = default);

    Task<Guid> UploadToCollectionAsync(
        Guid collectionId,
        Stream content,
        string contentType,
        string? originalFileName,
        long? knownSizeBytes,
        CancellationToken cancellationToken = default);

    Task<FileMetadataDto?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileMetadataDto>> ListByCollectionAsync(Guid collectionId, CancellationToken cancellationToken = default);

    Task<FileDownloadResult?> OpenReadAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}

public sealed record FileDownloadResult(Stream Stream, string ContentType, string? FileDownloadName);
