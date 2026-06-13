namespace Nirbi.ServiceAuth.Http;

public sealed class ServiceTokenRequestDto
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string GrantType { get; set; } = "client_credentials";
}

public sealed class ServiceRegisterDto
{
    public string ServiceId { get; set; }
    public string ServiceName { get; set; }
    public string Description { get; set; }
    public List<string> Scopes { get; set; }

    /// <summary>
    /// If set, this value is stored as the service client secret.
    /// If null or empty, a random secret is generated and returned in the registration response.
    /// </summary>
    public string ClientSecret { get; set; }
}

public class ServiceRegistrationResponse
{
    public string ServiceId { get; set; }
    public string ServiceName { get; set; }
    public string Description { get; set; }
    public string ClientSecret { get; set; }
    public List<string> Scopes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ServiceTokenResponseDto
{
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
}
