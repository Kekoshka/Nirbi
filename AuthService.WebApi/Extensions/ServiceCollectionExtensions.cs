using AuthService.WebApi.Configuration;
using AuthService.WebApi.Data.Extensions;
using AuthService.WebApi.Domain.Services;
using AuthService.WebApi.External.Keycloak;
using AuthService.WebApi.Hosting;
using AuthService.WebApi.Utilities;

namespace AuthService.WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthServiceDependencies(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuration
            services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.Section));
            services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.Section));
            services.Configure<ServiceTokenOptions>(configuration.GetSection(ServiceTokenOptions.Section));
            services.Configure<ServiceRegistryOptions>(configuration.GetSection(ServiceRegistryOptions.Section));

            services.AddHostedService<ServiceRegistryInitializer>();

            // Data Access
            services.AddDataAccess(configuration);

            // External Services
            services.AddKeycloakIntegration(configuration);

            // Domain Services
            services.AddScoped<IKeycloakIntegrationService, KeycloakIntegrationService>();
            services.AddScoped<IServiceTokenService, ServiceTokenService>();

            // Utilities
            services.AddSingleton<PasswordHasher>();
            services.AddSingleton<JwtTokenGenerator>();

            // Caching
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });

            return services;
        }
    }
}
