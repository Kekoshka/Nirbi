namespace AuthService.WebApi.External.Keycloak.Models;

/// <summary>
/// Ответ списка пользователей с пагинацией.
/// items — массив объектов с запрошенными через ?fields= полями (+ всегда id).
/// </summary>
public class PagedUsersResponse
{
    public int Total { get; set; }
    public List<Dictionary<string, string?>> Items { get; set; } = [];
}
