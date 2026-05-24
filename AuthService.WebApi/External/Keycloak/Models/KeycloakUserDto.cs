using System.Text.Json.Serialization;

namespace AuthService.WebApi.External.Keycloak.Models
{
    using System.Text.Json.Serialization;

    public class KeycloakUserDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("credentials")]
        public IEnumerable<KeycloakCredential>? Credentials { get; set; }

        // Все кастомные атрибуты здесь
        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }
    }

    public static class KeycloakUserExtensions
    {
        public static KeycloakUserDto ToKeycloakRequest(
            this UserProfile profile,
            string password,
            bool emailAsUsername = true)
        {
            var username = profile.Email;

            var request = new KeycloakUserDto
            {
                Username = username,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Enabled = true,
                Credentials = new[]
                {
                new KeycloakCredential
                {
                    Type = "password",
                    Value = password,
                    Temporary = false
                }
            },
                Attributes = new Dictionary<string, List<string>>()
            };
            AddAttributeIfPresent(request.Attributes, "secondName", profile.SecondName);
            AddAttributeIfPresent(request.Attributes, "phone", profile.Phone);
            AddAttributeIfPresent(request.Attributes, "birthDate", profile.BirthDate);
            AddAttributeIfPresent(request.Attributes, "city", profile.City);
            AddAttributeIfPresent(request.Attributes, "about", profile.About);
            AddAttributeIfPresent(request.Attributes, "educationPlace", profile.EducationPlace);
            AddAttributeIfPresent(request.Attributes, "educationStartYear", profile.EducationStartYear);
            AddAttributeIfPresent(request.Attributes, "educationEndYear", profile.EducationEndYear);
            AddAttributeIfPresent(request.Attributes, "educationField", profile.EducationField);

            return request;
        }

        private static void AddAttributeIfPresent(
            Dictionary<string, List<string>> attributes,
            string key,
            string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                attributes[key] = new List<string> { value };
            }
        }

        public static UserProfile ToUserProfile(this KeycloakUserDto dto)
        {
            if (dto == null) return null;

            return new UserProfile
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                SecondName = GetAttributeValue(dto.Attributes, "secondName"),
                Phone = GetAttributeValue(dto.Attributes, "phone"),
                BirthDate = GetAttributeValue(dto.Attributes, "birthDate"),
                City = GetAttributeValue(dto.Attributes, "city"),
                About = GetAttributeValue(dto.Attributes, "about"),
                EducationPlace = GetAttributeValue(dto.Attributes, "educationPlace"),
                EducationStartYear = GetAttributeValue(dto.Attributes, "educationStartYear"),
                EducationEndYear = GetAttributeValue(dto.Attributes, "educationEndYear"),
                EducationField = GetAttributeValue(dto.Attributes, "educationField")
            };
        }

        private static string? GetAttributeValue(Dictionary<string, List<string>>? attributes, string key)
        {
            if (attributes != null && attributes.TryGetValue(key, out var values) && values != null && values.Count > 0)
                return values[0];
            return null;
        }
    }

    public class UserProfile
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? BirthDate { get; set; }
        public string? City { get; set; }
        public string? About { get; set; }
        public string? EducationPlace { get; set; }
        public string? EducationStartYear { get; set; }
        public string? EducationEndYear { get; set; }
        public string? EducationField { get; set; }
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
