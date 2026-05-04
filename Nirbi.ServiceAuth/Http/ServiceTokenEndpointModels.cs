namespace Nirbi.ServiceAuth.Http;

public sealed class ServiceTokenRequestDto
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string GrantType { get; set; } = "client_credentials";
}

public sealed class ServiceTokenResponseDto
{
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
}
