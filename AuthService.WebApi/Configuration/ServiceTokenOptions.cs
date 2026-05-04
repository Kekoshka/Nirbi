namespace AuthService.WebApi.Configuration
{
    public class ServiceTokenOptions
    {
        public const string Section = "ServiceTokens";

        public int AccessTokenExpirationMinutes { get; set; } = 60;
        public int MaxTokenCacheMinutes { get; set; } = 50;
    }
}
