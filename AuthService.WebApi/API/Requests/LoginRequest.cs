namespace AuthService.WebApi.API.Requests
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ServiceTokenRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; } = "client_credentials";
    }

    public class RegisterServiceRequest
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

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }

    public class LogoutRequest
    {
        public string RefreshToken { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class UpdateScopesRequest
    {
        public List<string> Scopes { get; set; }
    }
}
