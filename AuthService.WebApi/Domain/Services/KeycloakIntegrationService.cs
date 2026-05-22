using AuthService.WebApi.API.Responses;
using AuthService.WebApi.Configuration;
using AuthService.WebApi.External.Keycloak;
using AuthService.WebApi.External.Keycloak.Models;
using ExceptionHandler.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Refit;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace AuthService.WebApi.Domain.Services;

public class KeycloakIntegrationService : IKeycloakIntegrationService
{
    private readonly IKeycloakClient _keycloakClient;
    private readonly KeycloakOptions _keycloakOptions;
    private readonly IDistributedCache _cache;
    private string _adminAccessToken;
    private DateTime _adminTokenExpiration;

    public KeycloakIntegrationService(
        IKeycloakClient keycloakClient,
        IOptions<KeycloakOptions> keycloakOptions,
        IDistributedCache cache)
    {
        _keycloakClient = keycloakClient;
        _keycloakOptions = keycloakOptions.Value;
        _cache = cache;
    }

    public async Task<AuthResponseDto> LoginAsync(
        string username, string password,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", _keycloakOptions.PublicClientId },
            { "username", username },
            { "password", password }
        };

        var response = await _keycloakClient.GetTokenAsync(
            _keycloakOptions.Realm, parameters, cancellationToken);

        return ToAuthResponse(response);
    }

    public async Task<AuthResponseDto> RegisterAsync(
        string username, string email, string password,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var user = new KeycloakUserDto
        {
            Username = username,
            Email = email,
            Enabled = true,
            Credentials = new[]
            {
                new KeycloakCredential { Type = "password", Value = password, Temporary = false }
            }
        };

        try
        {
            await _keycloakClient.CreateUserAsync(
                _keycloakOptions.Realm, $"Bearer {adminToken}", user, cancellationToken);
        }
        catch (ApiException ex)
        {
            var content = await ex.GetContentAsAsync<object>();
            Console.Write("Keycloak error:" + content);
            throw;
        }

        return await LoginAsync(username, password, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", _keycloakOptions.PublicClientId },
            { "refresh_token", refreshToken }
        };

        var response = await _keycloakClient.GetTokenAsync(
            _keycloakOptions.Realm, parameters, cancellationToken);

        return ToAuthResponse(response);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _keycloakOptions.PublicClientId },
            { "refresh_token", refreshToken }
        };

        await _keycloakClient.LogoutAsync(_keycloakOptions.Realm, parameters, cancellationToken);
    }

    public async Task<string> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var users = await _keycloakClient.SearchUsersByEmailAsync(
            _keycloakOptions.Realm, $"Bearer {adminToken}", email, cancellationToken);

        var user = users.FirstOrDefault()
            ?? throw new InvalidOperationException("User not found");

        await _keycloakClient.ExecuteActionsEmailAsync(
            _keycloakOptions.Realm, user.Id, $"Bearer {adminToken}",
            new[] { "UPDATE_PASSWORD" }, cancellationToken);

        return "Password reset email sent";
    }

    public async Task<IEnumerable<UserSearchResultDto>> SearchUsersByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var users = await _keycloakClient.SearchUsersByUsernameAsync(
            _keycloakOptions.Realm, $"Bearer {adminToken}", username, cancellationToken);

        return users
            .Where(u => u.Id is not null)
            .Select(u => new UserSearchResultDto(Guid.Parse(u.Id), u.Username));
    }

    public async Task<UserSearchResultDto?> GetUserByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        var user = await _keycloakClient.GetUserByIdAsync(
            _keycloakOptions.Realm, userId, $"Bearer {adminToken}", cancellationToken);

        if (user?.Id is null)
            throw new NotFoundException($"User with id {userId} not found");
        return new UserSearchResultDto(Guid.Parse(user.Id), user.Username);
    }


    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Извлекает UserId (claim "sub") из AccessToken без лишнего запроса к Keycloak.
    /// </summary>
    private static AuthResponseDto ToAuthResponse(KeycloakTokenResponse response)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(response.AccessToken);
        var sub = jwt.Subject
            ?? throw new InvalidOperationException("JWT does not contain 'sub' claim.");

        return new AuthResponseDto(
            UserId: Guid.Parse(sub),
            AccessToken: response.AccessToken,
            RefreshToken: response.RefreshToken,
            TokenType: response.TokenType,
            ExpiresIn: response.ExpiresIn);
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_adminAccessToken) && DateTime.UtcNow < _adminTokenExpiration)
            return _adminAccessToken;

        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _keycloakOptions.AdminClientId },
            { "client_secret", _keycloakOptions.AdminClientSecret }
        };

        var response = await _keycloakClient.GetAdminTokenAsync(parameters, cancellationToken);

        _adminAccessToken = response.AccessToken;
        _adminTokenExpiration = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 60);

        return _adminAccessToken;
    }
}