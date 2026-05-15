using System.Text.Json;
using System.Text.Json.Nodes;
using AuthService.WebApi.API.Requests;
using AuthService.WebApi.Configuration;
using AuthService.WebApi.Domain.Services;
using Microsoft.Extensions.Options;

namespace AuthService.WebApi.Hosting
{
    public sealed class ServiceRegistryInitializer : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<ServiceRegistryOptions> _options;
        private readonly ILogger<ServiceRegistryInitializer> _logger;

        public ServiceRegistryInitializer(
            IServiceScopeFactory scopeFactory,
            IOptions<ServiceRegistryOptions> options,
            ILogger<ServiceRegistryInitializer> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var opts = _options.Value;
            if (!opts.Enabled || opts.Services.Count == 0)
                return;

            using var scope = _scopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<IServiceTokenService>();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var fileName = env == "Production" ? "appsettings.json" : $"appsettings.{env}.json";
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = File.ReadAllText(jsonPath);
            var node = JsonNode.Parse(json)!;
            var servicesArray = node["ServiceRegistry"]!["Services"]!.AsArray();
            foreach (var (entry, index) in opts.Services.Select((s, i) => (s, i)))
            {
                if (string.IsNullOrWhiteSpace(entry.ServiceId) || string.IsNullOrWhiteSpace(entry.ServiceName))
                {
                    _logger.LogWarning("ServiceRegistry: skipping entry with missing ServiceId or ServiceName.");
                    continue;
                }

                try
                {
                    var existing = await tokenService.GetServiceAsync(entry.ServiceId, cancellationToken)
                        .ConfigureAwait(false);


                    if (existing == null)
                    {
                        var request = new RegisterServiceRequest
                        {
                            ServiceId = entry.ServiceId.Trim(),
                            ServiceName = entry.ServiceName.Trim(),
                            Description = entry.Description?.Trim() ?? string.Empty,
                            Scopes = entry.Scopes ?? new List<string>(),
                            ClientSecret = entry.ClientSecret
                        };

                        var created = await tokenService.RegisterServiceAsync(request, cancellationToken)
                            .ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(entry.ClientSecret))
                        {
                            _logger.LogWarning(
                                "ServiceRegistry: registered {ServiceId}. Save this client secret: {ClientSecret}",
                                created.ServiceId,
                                created.ClientSecret);
                            var serviceNode = servicesArray[index];
                            if (serviceNode != null)
                            {
                                serviceNode["ClientSecret"] = created.ClientSecret;
                            }
                        }
                        else
                        {
                            _logger.LogInformation(
                                "ServiceRegistry: registered {ServiceId} with configured client secret.",
                                created.ServiceId);
                        }
                    }
                    else if (opts.SyncScopesOnStartup && entry.Scopes is { Count: > 0 })
                    {
                        await tokenService.UpdateServiceScopesAsync(
                                entry.ServiceId,
                                entry.Scopes,
                                cancellationToken)
                            .ConfigureAwait(false);

                        _logger.LogInformation(
                            "ServiceRegistry: updated scopes for {ServiceId}.",
                            entry.ServiceId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "ServiceRegistry: failed to provision service {ServiceId}.",
                        entry.ServiceId);
                }
            }
            await File.WriteAllTextAsync(jsonPath, 
                node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
                cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
