using Amazon.S3;
using Amazon.S3.Util;
using DataService.WebApi.Common.Options;
using Microsoft.Extensions.Options;

namespace DataService.WebApi.Hosting;

public sealed class EnsureS3BucketHostedService : IHostedService
{
    private readonly IAmazonS3 _s3;
    private readonly S3StorageOptions _options;
    private readonly ILogger<EnsureS3BucketHostedService> _logger;

    public EnsureS3BucketHostedService(
        IAmazonS3 s3,
        IOptions<S3StorageOptions> options,
        ILogger<EnsureS3BucketHostedService> logger)
    {
        _s3 = s3;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BucketName))
            return;

        var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3, _options.BucketName).ConfigureAwait(false);
        if (exists)
            return;

        _logger.LogInformation("Creating S3 bucket {Bucket}", _options.BucketName);
        await _s3.PutBucketAsync(_options.BucketName, cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
