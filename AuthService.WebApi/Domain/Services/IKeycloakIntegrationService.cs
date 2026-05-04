using AuthService.WebApi.External.Keycloak.Models;

namespace AuthService.WebApi.Domain.Services
{
    public interface IKeycloakIntegrationService
    {
        Task<KeycloakTokenResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
        Task<KeycloakTokenResponse> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default);
        Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<string> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    }
}
