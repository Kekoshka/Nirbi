namespace DataService.WebApi.Common.Options;

public class S3StorageOptions
{
    public const string SectionName = "S3Storage";

    public string BucketName { get; set; } = string.Empty;

    /// <summary>Optional custom endpoint (MinIO, LocalStack). Leave empty for AWS.</summary>
    public string? ServiceUrl { get; set; }

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>AWS region when <see cref="ServiceUrl"/> is not set.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Required for MinIO and many S3-compatible stores.</summary>
    public bool ForcePathStyle { get; set; } = true;
}
