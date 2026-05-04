using AuthService.WebApi.API.Requests;
using AuthService.WebApi.API.Responses;

namespace AuthService.WebApi.Domain.Services
{
    public interface IServiceTokenService
    {
        Task<ServiceRegistrationResponse> RegisterServiceAsync(
            RegisterServiceRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceTokenResponse> IssueServiceTokenAsync(
            ServiceTokenRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceDetailsResponse> GetServiceAsync(
            string serviceId,
            CancellationToken cancellationToken = default);

        Task UpdateServiceScopesAsync(
            string serviceId,
            List<string> scopes,
            CancellationToken cancellationToken = default);

        Task RevokeServiceAsync(
            string serviceId,
            CancellationToken cancellationToken = default);
    }
}
