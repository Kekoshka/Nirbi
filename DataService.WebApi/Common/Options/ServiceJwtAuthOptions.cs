namespace DataService.WebApi.Common.Options;

/// <summary>Symmetric JWT validation (same shape as Nirbi AuthService internal tokens).</summary>
public class ServiceJwtAuthOptions
{
    public const string SectionPath = "NirbiServiceAuth:ServiceJwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
}
