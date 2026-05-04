namespace Nirbi.ServiceAuth.Configuration;

public class ServiceAuthOptions
{
    public const string SectionName = "NirbiServiceAuth";

    public string AuthServiceBaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public int TokenRefreshSkewSeconds { get; set; } = 120;
    public ServiceJwtOptions ServiceJwt { get; set; } = new();
    public KeycloakJwtOptions? Keycloak { get; set; }
}

public class ServiceJwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
}

public class KeycloakJwtOptions
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
}
