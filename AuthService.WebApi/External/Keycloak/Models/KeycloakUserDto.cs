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

        public static Dictionary<string, string> ToFieldDict(this KeycloakUserDto dto, List<string> fields)
        {
            if (dto == null) return null;

            Dictionary<string, string> response = new Dictionary<string, string>();
            foreach (var field in fields)
            {
                switch (field)
                {
                    case "firstName":
                        response.Add(field, dto.FirstName);
                        break;
                    case "lastName":
                        response.Add(field, dto.LastName);
                        break;
                    case "email":
                        response.Add(field, dto.Email);
                        break;
                    case "username":
                        response.Add(field, dto.Username);
                        break;
                    default:
                        response.Add(field, GetAttributeValue(dto.Attributes, field));
                        break;
                }
            }
            return response;
        }

        private static string? GetAttributeValue(Dictionary<string, List<string>>? attributes, string key)
        {
            if (attributes != null && attributes.TryGetValue(key, out var values) && values != null && values.Count > 0)
                return values[0];
            return null;
        }

        public static KeycloakUserDto ToKeycloakUpdateRequest(
        this UpdateUserRequest request,
        KeycloakUserDto existing)
        {
            var updateDto = new KeycloakUserDto
            {
                Id = existing.Id,
                Username = existing.Username,
                Email = existing.Email,
                FirstName = existing.FirstName,
                LastName = existing.LastName,
                Enabled = existing.Enabled,
                Attributes = existing.Attributes != null
                    ? new Dictionary<string, List<string>>(existing.Attributes)
                    : new Dictionary<string, List<string>>()
            };

            if (request.FirstName != null)
                updateDto.FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? null : request.FirstName;
            if (request.LastName != null)
                updateDto.LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName;
            if (request.Email != null)
            {
                updateDto.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;
                updateDto.Username = updateDto.Email;
            }

            UpdateAttributeForPartialUpdate(updateDto.Attributes, "secondName", request.SecondName);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "phone", request.Phone);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "birthDate", request.BirthDate);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "city", request.City);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "about", request.About);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "educationPlace", request.EducationPlace);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "educationStartYear", request.EducationStartYear);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "educationEndYear", request.EducationEndYear);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "educationField", request.EducationField);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "vk", request.vk);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "tg", request.tg);
            UpdateAttributeForPartialUpdate(updateDto.Attributes, "max", request.max);

            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                updateDto.Credentials = new[]
                {
                new KeycloakCredential
                {
                    Type = "password",
                    Value = request.NewPassword,
                    Temporary = false
                }
            };
            }

            return updateDto;
        }

        private static void UpdateAttributeForPartialUpdate(
            Dictionary<string, List<string>> attributes,
            string key,
            string? value)
        {
            if (value == null) return;

            if (string.IsNullOrWhiteSpace(value))
            {
                attributes.Remove(key);
            }
            else
            {
                attributes[key] = new List<string> { value };
            }
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
        public string? vk { get; set; }
        public string? tg { get; set; }
        public string? max { get; set; }
    }

    public class UpdateUserRequest : UserProfile
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
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

    public class UserFields
    {
        [JsonPropertyName("attributes")]
        public List<UserField> Attributes { get; set; }
    }

    public class UserField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
