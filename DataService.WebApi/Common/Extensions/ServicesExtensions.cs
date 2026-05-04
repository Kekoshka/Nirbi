using Amazon;
using Amazon.S3;
using DataService.DataAccess.Postgres.Context;
using DataService.WebApi.Common.Options;
using DataService.WebApi.Hosting;
using DataService.WebApi.Interfaces;
using DataService.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DataService.WebApi.Common.Extensions;

public static class ServicesExtensions
{
    private const string PostgreSqlConnectionName = "PostgreSql";

    public static void UsePostgreSql(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(PostgreSqlConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{PostgreSqlConnectionName}' is missing.");

        services.AddDbContext<DataDbContext>(options => options.UseNpgsql(connectionString));
    }

    public static void AddS3ObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3StorageOptions>(configuration.GetSection(S3StorageOptions.SectionName));
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<S3StorageOptions>>().Value;
            if (string.IsNullOrWhiteSpace(opts.BucketName))
                throw new InvalidOperationException("S3Storage:BucketName is required.");
            if (string.IsNullOrWhiteSpace(opts.AccessKey) || string.IsNullOrWhiteSpace(opts.SecretKey))
                throw new InvalidOperationException("S3Storage:AccessKey and SecretKey are required.");

            if (!string.IsNullOrWhiteSpace(opts.ServiceUrl))
            {
                var cfg = new AmazonS3Config
                {
                    ServiceURL = opts.ServiceUrl.TrimEnd('/'),
                    ForcePathStyle = opts.ForcePathStyle,
                };
                return new AmazonS3Client(opts.AccessKey, opts.SecretKey, cfg);
            }

            var region = RegionEndpoint.GetBySystemName(opts.Region);
            return new AmazonS3Client(opts.AccessKey, opts.SecretKey, region);
        });

        services.AddSingleton<IObjectStorageService, S3ObjectStorageService>();
        services.AddHostedService<EnsureS3BucketHostedService>();
    }

    public static void AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IDataObjectService, DataObjectService>();
    }
}
