namespace AuthService.WebApi.Configuration
{
    public class KeycloakOptions
    {
        public const string Section = "Keycloak";

        public string Url { get; set; }
        public string Realm { get; set; }
        public string AdminClientId { get; set; }
        public string AdminClientSecret { get; set; }
        public string PublicClientId { get; set; } = "myapp";

    }
}
