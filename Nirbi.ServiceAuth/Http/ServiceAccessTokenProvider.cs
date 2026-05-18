using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Extensions.Options;
using Nirbi.ServiceAuth.Configuration;

namespace Nirbi.ServiceAuth.Http;

public sealed class ServiceAccessTokenProvider : IServiceAccessTokenProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceAuthOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _cachedExpiresAt;

    public ServiceAccessTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<ServiceAuthOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var skew = TimeSpan.FromSeconds(Math.Max(0, _options.TokenRefreshSkewSeconds));
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedExpiresAt - skew)
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedExpiresAt - skew)
                return _cachedToken;

            var baseUrl = _options.AuthServiceBaseUrl.TrimEnd('/');
            var client = _httpClientFactory.CreateClient("Nirbi.ServiceAuth.Internal");
            using var response = await client.PostAsJsonAsync(
                $"{baseUrl}/api/ServiceToken/token",
                new ServiceTokenRequestDto
                {
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret,
                    GrantType = "client_credentials"
                },
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<ServiceTokenResponseDto>(
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);

            if (body?.AccessToken is null || string.IsNullOrWhiteSpace(body.AccessToken))
                throw new InvalidOperationException("AuthService returned an empty access token.");

            _cachedToken = body.AccessToken;
            _cachedExpiresAt = body.ExpiresIn > 0
                ? DateTimeOffset.UtcNow.AddSeconds(body.ExpiresIn)
                : DateTimeOffset.UtcNow.AddMinutes(55);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> RegisterServiceAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var baseUrl = _options.AuthServiceBaseUrl.TrimEnd('/');
            var client = _httpClientFactory.CreateClient("Nirbi.ServiceAuth.Internal");
            using var response = await client.PostAsJsonAsync(
                $"{baseUrl}/api/ServiceToken/register",
                new ServiceRegisterDto
                {
                    ServiceId = _options.ClientId,
                    ServiceName = _options.ServiceName,
                    Description = _options.Description,
                    Scopes = _options.Scopes,
                    ClientSecret = _options.ClientSecret,
                },
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<ServiceRegistrationResponse>(
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);

            if (body?.ClientSecret is null || string.IsNullOrWhiteSpace(body.ClientSecret))
                throw new InvalidOperationException("AuthService returned an empty token.");

            return body.ClientSecret;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAccessTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            if (!File.Exists(jsonPath)) throw new InvalidOperationException($"NirbiServiceAuth: appsettings.json not exists.");
            string json = File.ReadAllText(jsonPath);
            JsonNode? node = JsonNode.Parse(json)!;
            if (node == null) throw new InvalidOperationException($"NirbiServiceAuth: appsettings.json JsonNode is null.");
            if (node["NirbiServiceAuth"] == null) throw new InvalidOperationException($"NirbiServiceAuth: appsettings.json NirbiServiceAuth is null.");
            node["NirbiServiceAuth"]["ClientSecret"] = token;
            await File.WriteAllTextAsync(jsonPath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true })).ConfigureAwait(false);
        } 
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> GetAndSaveTokenAsync(CancellationToken cancellationToken = default)
    {
        string token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        await SaveAccessTokenAsync(token, cancellationToken).ConfigureAwait(false); 
        return token;
    }

    public async Task<string> RegisterAndSaveTokenAsync(CancellationToken cancellationToken = default)
    {
        string token = await RegisterServiceAsync(cancellationToken).ConfigureAwait(false);
        await SaveAccessTokenAsync(token, cancellationToken).ConfigureAwait(false);
        return token;
    }
}
