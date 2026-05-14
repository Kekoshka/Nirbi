namespace AuthService.WebApi.API.Responses
{
    public class ServiceTokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
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

    public class ServiceDetailsResponse
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public List<string> Scopes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }

    public class UserTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }
    public record AuthResponseDto(
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn);

    public record UserSearchResultDto(
        Guid UserId,
        string Username);

}
