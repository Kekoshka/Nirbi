using AuthService.WebApi.External.Keycloak.Models;
using Refit;

namespace AuthService.WebApi.External.Keycloak
{
    /// <summary>
    /// Refit client для взаимодействия с Keycloak
    /// </summary>
    public interface IKeycloakClient
    {
        /// <summary>
        /// Получает публичные ключи для валидации JWT
        /// </summary>
        [Get("/realms/{realm}/protocol/openid-connect/certs")]
        Task<KeycloakPublicKeysResponse> GetPublicKeysAsync(
            string realm,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает access token используя Resource Owner Password Flow
        /// </summary>
        [Post("/realms/{realm}/protocol/openid-connect/token")]
        Task<KeycloakTokenResponse> GetTokenAsync(
            string realm,
            [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает access token используя Client Credentials Flow (для админа)
        /// </summary>
        [Post("/realms/master/protocol/openid-connect/token")]
        Task<KeycloakTokenResponse> GetAdminTokenAsync(
            [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Создает нового пользователя
        /// </summary>
        [Post("/admin/realms/{realm}/users")]
        Task CreateUserAsync(
            string realm,
            [Header("Authorization")] string authHeader,
            [Body] KeycloakUserDto user,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает список пользователей по email
        /// </summary>
        [Get("/admin/realms/{realm}/users")]
        Task<IEnumerable<KeycloakUserDto>> SearchUsersByEmailAsync(
            string realm,
            [Header("Authorization")] string authHeader,
            [Query] string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает список пользователей по phone
        /// </summary>
        [Get("/admin/realms/{realm}/users")]
        Task<IEnumerable<KeycloakUserDto>> SearchUsersByPhoneAsync(
            string realm,
            [Header("Authorization")] string authHeader,
            [Query] string phone,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает пользователя по его уникальному идентификатору (UUID)
        /// </summary>
        /// <param name="realm">Название реалма</param>
        /// <param name="id">ID пользователя в Keycloak (Guid/string)</param>
        /// <param name="authHeader">Bearer токен администратора</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Объект пользователя или null, если не найден</returns>
        [Get("/admin/realms/{realm}/users/{id}")]
        Task<KeycloakUserDto> GetUserByIdAsync(
            string realm,
            string id,
            [Header("Authorization")] string authHeader,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполняет действия для пользователя (например, отправить email для сброса пароля)
        /// </summary>
        [Put("/admin/realms/{realm}/users/{userId}/execute-actions-email")]
        Task ExecuteActionsEmailAsync(
            string realm,
            string userId,
            [Header("Authorization")] string authHeader,
            [Body] IEnumerable<string> actions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполняет logout
        /// </summary>
        [Post("/realms/{realm}/protocol/openid-connect/logout")]
        Task LogoutAsync(
            string realm,
            [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters,
            CancellationToken cancellationToken = default);

        [Put("/admin/realms/{realm}/users/{id}")]
        Task UpdateUserAsync(
            string realm,
            string id,
            [Header("Authorization")] string authHeader,
            [Body] KeycloakUserDto user,
            CancellationToken cancellationToken);
    }
}
