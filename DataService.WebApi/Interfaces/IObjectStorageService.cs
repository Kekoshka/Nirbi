namespace DataService.WebApi.Interfaces;

public interface IObjectStorageService
{
    Task PutObjectAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> GetObjectStreamAsync(string storageKey, CancellationToken cancellationToken = default);

    Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken = default);
}
