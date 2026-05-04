using AuthService.WebApi.API.Requests;
using AuthService.WebApi.API.Responses;
using AuthService.WebApi.Configuration;
using AuthService.WebApi.Domain.Entities;
using AuthService.WebApi.Domain.Repositories;
using AuthService.WebApi.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace AuthService.WebApi.Domain.Services
{
    public class ServiceTokenService : IServiceTokenService
    {
        private readonly IServiceRepository _repository;
        private readonly AuthOptions _authOptions;
        private readonly ServiceTokenOptions _tokenOptions;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtTokenGenerator _tokenGenerator;

        public ServiceTokenService(
            IServiceRepository repository,
            IOptions<AuthOptions> authOptions,
            IOptions<ServiceTokenOptions> tokenOptions,
            PasswordHasher passwordHasher,
            JwtTokenGenerator tokenGenerator)
        {
            _repository = repository;
            _authOptions = authOptions.Value;
            _tokenOptions = tokenOptions.Value;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<ServiceRegistrationResponse> RegisterServiceAsync(
            RegisterServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException($"Service {request.ServiceId} already exists");

            var clientSecret = string.IsNullOrWhiteSpace(request.ClientSecret)
                ? GenerateClientSecret()
                : request.ClientSecret.Trim();
            var hashedSecret = _passwordHasher.Hash(clientSecret);

            var service = new ServiceEntity
            {
                Id = request.ServiceId,
                Name = request.ServiceName,
                Description = request.Description,
                ClientSecret = hashedSecret,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                AllowedScopes = request.Scopes
                    .Select(scope => new ServiceScopeEntity { Scope = scope })
                    .ToList()
            };

            await _repository.AddAsync(service, cancellationToken);

            return new ServiceRegistrationResponse
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                Description = service.Description,
                ClientSecret = clientSecret,
                Scopes = request.Scopes,
                IsActive = true,
                CreatedAt = service.CreatedAt
            };
        }

        public async Task<ServiceTokenResponse> IssueServiceTokenAsync(
            ServiceTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            var service = await _repository.GetByIdAsync(request.ClientId, cancellationToken);

            if (service == null)
                throw new InvalidOperationException("Service not found");

            if (!service.IsActive)
                throw new InvalidOperationException("Service is not active");

            if (!_passwordHasher.Verify(request.ClientSecret, service.ClientSecret))
                throw new InvalidOperationException("Invalid client secret");

            var scopes = service.AllowedScopes.Select(s => s.Scope).ToList();
            var token = _tokenGenerator.GenerateServiceToken(
                service.Id,
                scopes,
                _authOptions.AccessTokenExpirationMinutes
            );

            return new ServiceTokenResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = _authOptions.AccessTokenExpirationMinutes * 60,
                Scope = string.Join(" ", scopes)
            };
        }

        public async Task<ServiceDetailsResponse> GetServiceAsync(
            string serviceId,
            CancellationToken cancellationToken = default)
        {
            var service = await _repository.GetByIdAsync(serviceId, cancellationToken);

            if (service == null)
                return null;

            return new ServiceDetailsResponse
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                Description = service.Description,
                Scopes = service.AllowedScopes.Select(s => s.Scope).ToList(),
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                DeactivatedAt = service.DeactivatedAt
            };
        }

        public async Task UpdateServiceScopesAsync(
            string serviceId,
            List<string> scopes,
            CancellationToken cancellationToken = default)
        {
            var service = await _repository.GetByIdAsync(serviceId, cancellationToken);

            if (service == null)
                throw new InvalidOperationException("Service not found");

            service.AllowedScopes.Clear();
            service.AllowedScopes = scopes
                .Select(scope => new ServiceScopeEntity { Scope = scope })
                .ToList();

            await _repository.UpdateAsync(service, cancellationToken);
        }

        public async Task RevokeServiceAsync(
            string serviceId,
            CancellationToken cancellationToken = default)
        {
            var service = await _repository.GetByIdAsync(serviceId, cancellationToken);

            if (service == null)
                throw new InvalidOperationException("Service not found");

            service.IsActive = false;
            service.DeactivatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(service, cancellationToken);
        }

        private string GenerateClientSecret()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
