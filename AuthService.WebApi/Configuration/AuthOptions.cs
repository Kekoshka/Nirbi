namespace AuthService.WebApi.Configuration
{
    public class AuthOptions
    {
        public const string Section = "Auth";

        public string Issuer { get; set; } = "myapp-auth";
        public string Audience { get; set; } = "myapp-services";
        public string SecretKey { get; set; }
        public int AccessTokenExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 30;

    }
}
