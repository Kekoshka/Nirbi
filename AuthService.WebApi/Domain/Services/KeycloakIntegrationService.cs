using AuthService.WebApi.Configuration;
using AuthService.WebApi.External.Keycloak;
using AuthService.WebApi.External.Keycloak.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Refit;

namespace AuthService.WebApi.Domain.Services
{
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

        public async Task<KeycloakTokenResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _keycloakOptions.PublicClientId },
                { "username", username },
                { "password", password }
            };

            var response = await _keycloakClient.GetTokenAsync(
                _keycloakOptions.Realm,
                parameters,
                cancellationToken
            );

            return response;
        }

        public async Task<KeycloakTokenResponse> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default)
        {
            var adminToken = await GetAdminTokenAsync(cancellationToken);

            var user = new External.Keycloak.Models.KeycloakUserDto
            {
                Username = username,
                Email = email,
                Enabled = true,
                Credentials = new[]
                {
                    new External.Keycloak.Models.KeycloakCredential
                    {
                        Type = "password",
                        Value = password,
                        Temporary = false
                    }
                }
            };
            try
            {
                await _keycloakClient.CreateUserAsync(
                    _keycloakOptions.Realm,
                    $"Bearer {adminToken}",
                    user,
                    cancellationToken);
            }
            catch (ApiException ex)
            {
                var content = await ex.GetContentAsAsync<object>();
                Console.Write("Keycloak error:" +  content);
                throw;

            }

            return await LoginAsync(username, password, cancellationToken);
        }

        public async Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", _keycloakOptions.PublicClientId },
                { "refresh_token", refreshToken }
            };

            var response = await _keycloakClient.GetTokenAsync(
                _keycloakOptions.Realm,
                parameters,
                cancellationToken
            );

            return response;
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _keycloakOptions.PublicClientId },
                { "refresh_token", refreshToken }
            };

            await _keycloakClient.LogoutAsync(
                _keycloakOptions.Realm,
                parameters,
                cancellationToken
            );
        }

        public async Task<string> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {
            var adminToken = await GetAdminTokenAsync(cancellationToken);

            var users = await _keycloakClient.SearchUsersByEmailAsync(
                _keycloakOptions.Realm,
                $"Bearer {adminToken}",
                email,
                cancellationToken
            );

            var user = users.FirstOrDefault();
            if (user == null)
                throw new InvalidOperationException("User not found");

            var userId = user.Id;

            await _keycloakClient.ExecuteActionsEmailAsync(
                _keycloakOptions.Realm,
                userId,
                $"Bearer {adminToken}",
                new[] { "UPDATE_PASSWORD" },
                cancellationToken
            );

            return "Password reset email sent";
        }

        private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default)
        {
            // Проверяем кэш
            if (!string.IsNullOrEmpty(_adminAccessToken) && DateTime.UtcNow < _adminTokenExpiration)
                return _adminAccessToken;

            // Если AdminClientSecret пустой - используем master realm с admin user
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
}
