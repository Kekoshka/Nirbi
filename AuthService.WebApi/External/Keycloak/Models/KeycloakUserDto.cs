using System.Text.Json.Serialization;

namespace AuthService.WebApi.External.Keycloak.Models
{
    public class KeycloakUserDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("credentials")]
        public IEnumerable<KeycloakCredential> Credentials { get; set; }
    }

    public class KeycloakCredential
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "password";

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("temporary")]
        public bool Temporary { get; set; } = false;
    }
}
