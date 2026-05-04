using AuthService.WebApi.Configuration;
using Refit;

namespace AuthService.WebApi.External.Keycloak
{
    public static class KeycloakServiceExtensions
    {
        public static IServiceCollection AddKeycloakIntegration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var keycloakOptions = new KeycloakOptions();
            configuration.GetSection(KeycloakOptions.Section).Bind(keycloakOptions);

            if (string.IsNullOrEmpty(keycloakOptions.Url))
                throw new InvalidOperationException("Keycloak:Url is required");

            services.AddSingleton(Microsoft.Extensions.Options.Options.Create(keycloakOptions));

            // Просто регистрируем Refit client без Polly
            services.AddRefitClient<IKeycloakClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(keycloakOptions.Url);
                    c.Timeout = TimeSpan.FromSeconds(30);
                });

            return services;
        }
    }
}
