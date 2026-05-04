using System.Net.Http.Json;
using System.Text.Json;
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
}
