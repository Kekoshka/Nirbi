using AuthService.WebApi.API.Responses;
using AuthService.WebApi.External.Keycloak.Models;

namespace AuthService.WebApi.Domain.Services
{
    public interface IKeycloakIntegrationService
    {
        Task<AuthResponseDto> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<string> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserSearchResultDto>> SearchUsersByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
