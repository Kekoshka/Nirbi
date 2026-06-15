using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading;
using AuthService.WebApi.API.Responses;
using AuthService.WebApi.Configuration;
using AuthService.WebApi.External.Keycloak;
using AuthService.WebApi.External.Keycloak.Models;
using AuthService.WebApi.Utilities;
using ExceptionHandler.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Refit;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    string emailOrPhone, string password,
    CancellationToken cancellationToken = default)
    {
        string username;

        if (emailOrPhone.Contains("@"))
        {
            username = emailOrPhone;
        }
        else
        {
            var adminToken = await GetAdminTokenAsync(cancellationToken);
            var users = await _keycloakClient.SearchUsersByPhoneAsync(
                _keycloakOptions.Realm,
                $"Bearer {adminToken}",
                $"phone:{emailOrPhone}",
                cancellationToken);

            var user = users?.FirstOrDefault();
            if (user == null)
            {
                throw new Exception("Пользователь с таким телефоном не найден");
            }

            username = user.Email ?? user.Username;
            if (string.IsNullOrEmpty(username))
            {
                throw new Exception("У пользователя не указан email или username");
            }
        }

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _keycloakOptions.PublicClientId,
            ["username"] = username,
            ["password"] = password
        };

        var response = await _keycloakClient.GetTokenAsync(
            _keycloakOptions.Realm,
            parameters,
            cancellationToken);

        return await ToAuthResponse(response, cancellationToken);
    }

    public async Task<AuthResponseDto> RegisterAsync(
    string FName, string SName, string LName, string phone, string email, string password,
    CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var userProfile = new UserProfile
        {
            FirstName = FName,
            SecondName = SName,
            LastName = LName,
            Phone = phone,
            Email = email
        };

        var keycloakUser = userProfile.ToKeycloakRequest(password, emailAsUsername: true);

        try
        {
            await _keycloakClient.CreateUserAsync(
                _keycloakOptions.Realm,
                $"Bearer {adminToken}",
                keycloakUser,
                cancellationToken);
        }
        catch (ApiException ex)
        {
            var content = await ex.GetContentAsAsync<object>();
            Console.WriteLine("Keycloak error:" + content);
            throw;
        }

        return await LoginAsync(email, password, cancellationToken);
    }

    public async Task<bool> UpdateUser(UpdateUserRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await GetUserInfo(data.Id, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"User with id '{data.Id}' not found.");

            if (string.IsNullOrEmpty(data.CurrentPassword))
                throw new Exception("Current password parameter is required.");

            var isValid = await VerifyUserPasswordAsync(
                existing.Email ?? existing.Username,
                data.CurrentPassword,
                cancellationToken);
            if (!isValid)
                throw new Exception("Current password is incorrect.");

            var updateDto = data.ToKeycloakUpdateRequest(existing);

            var adminToken = await GetAdminTokenAsync(cancellationToken);
            await _keycloakClient.UpdateUserAsync(
                _keycloakOptions.Realm,
                data.Id,
                $"Bearer {adminToken}",
                updateDto,
                cancellationToken);

            return true;
        }
        catch (ApiException ex)
        {
            var content = await ex.GetContentAsAsync<object>();
            Console.WriteLine("Keycloak error:" + content);
            throw;
        }
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

        return await ToAuthResponse(response, cancellationToken);
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
        string email,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var users = await _keycloakClient.SearchUsersByEmailAsync(
            _keycloakOptions.Realm, $"Bearer {adminToken}", email, cancellationToken);

        return users
            .Where(u => u.Id is not null)
            .Select(u => new UserSearchResultDto(Guid.Parse(u.Id), u.Email));
    }

    public async Task<UserSearchResultDto?> GetUserByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        var user = await _keycloakClient.GetUserByIdAsync(
            _keycloakOptions.Realm, userId.ToString(), $"Bearer {adminToken}", cancellationToken);

        if (user?.Id is null)
            throw new NotFoundException($"User with id {userId} not found");
        return new UserSearchResultDto(Guid.Parse(user.Id), user.Username);
    }

    public async Task<UserFields> GetUserProfileSchemaAsync(CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        UserFields upConfig = await _keycloakClient.GetUserProfileConfigurationAsync(
            _keycloakOptions.Realm,
            $"Bearer {adminToken}",
            cancellationToken);

        return upConfig;
    }


    // ── helpers ───────────────────────────────────────────────────────────────

    public async Task<bool> VerifyUserPasswordAsync(string username, string password, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _keycloakOptions.PublicClientId,
            ["username"] = username,
            ["password"] = password
        };
        try
        {
            var token = await _keycloakClient.GetTokenAsync(_keycloakOptions.Realm, parameters, ct);
            return !string.IsNullOrEmpty(token?.AccessToken);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }
    }

    public async Task<KeycloakUserDto> GetUserInfo(string id, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        var userInfo = await _keycloakClient.GetUserByIdAsync(_keycloakOptions.Realm, id, $"Bearer {adminToken}", cancellationToken);
        userInfo.Credentials = null;
        return userInfo;
    }

    public async Task<(IReadOnlyList<KeycloakUserDto> Users, int Total)> ListUsersAsync(
    int offset, int limit, string? search, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        var bearer = $"Bearer {adminToken}";

        // Нормализуем search: пустую строку шлём как null (Keycloak вернёт всех)
        var searchParam = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        // Список и счётчик параллельно — оба используют один admin-токен.
        var usersTask = _keycloakClient.GetUsersAsync(
            _keycloakOptions.Realm, bearer, offset, limit, searchParam, cancellationToken);
        var countTask = _keycloakClient.GetUsersCountAsync(
            _keycloakOptions.Realm, bearer, searchParam, cancellationToken);

        await Task.WhenAll(usersTask, countTask);

        var users = await usersTask ?? [];
        var total = await countTask;

        // Чистим credentials на всякий случай (как в GetUserInfo)
        foreach (var u in users)
            u.Credentials = null;

        return (users, total);
    }


    /// <summary>
    /// Извлекает UserId (claim "sub") из AccessToken без лишнего запроса к Keycloak.
    /// </summary>
    private async Task<AuthResponseDto> ToAuthResponse(KeycloakTokenResponse response, CancellationToken cancellationToken = default)
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