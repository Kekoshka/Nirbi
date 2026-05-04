using Amazon.S3;
using Amazon.S3.Model;
using DataService.WebApi.Common.Options;
using DataService.WebApi.Interfaces;
using Microsoft.Extensions.Options;

namespace DataService.WebApi.Services;

public sealed class S3ObjectStorageService : IObjectStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly S3StorageOptions _options;

    public S3ObjectStorageService(IAmazonS3 s3, IOptions<S3StorageOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task PutObjectAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        if (content.CanSeek && content.Position != 0)
            content.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        await _s3.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream> GetObjectStreamAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        using var response = await _s3.GetObjectAsync(_options.BucketName, storageKey, cancellationToken).ConfigureAwait(false);
        var buffer = new MemoryStream();
        await response.ResponseStream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;
        return buffer;
    }

    public Task DeleteObjectAsync(string storageKey, CancellationToken cancellationToken = default) =>
        _s3.DeleteObjectAsync(_options.BucketName, storageKey, cancellationToken);
}
