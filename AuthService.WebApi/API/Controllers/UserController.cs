using System.Linq;
using AuthService.WebApi.Configuration;
using AuthService.WebApi.Domain.Services;
using AuthService.WebApi.External.Keycloak;
using AuthService.WebApi.External.Keycloak.Models;
using AuthService.WebApi.Utilities;
using ExceptionHandler.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.WebApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IKeycloakIntegrationService _keycloakService;
    private readonly PasswordHasher _passwordHasher;

    public UsersController(IKeycloakIntegrationService keycloakService, PasswordHasher passwordHasher)
    {
        _keycloakService = keycloakService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Поиск пользователей по username (частичное совпадение поддерживается Keycloak)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string username,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("Username query parameter is required.");

        var results = await _keycloakService.SearchUsersByUsernameAsync(
            username, cancellationToken);

        return Ok(results);
    }

    /// <summary>
    /// Получение информации о пользователе по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(
        string id, [FromQuery] List<string>? fields,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Id parameter is required.");
        }

        if (!Guid.TryParseExact(id, "D", out _))
        {
            return BadRequest("Id must be a valid GUID in format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
        }

        var user = await _keycloakService.GetUserInfo(id, cancellationToken);

        if (user == null)
        {
            return NotFound($"User with id '{id}' not found.");
        }

        if (fields == null || fields.Count == 0)
        {
            return Ok(KeycloakUserExtensions.ToUserProfile(user));
        }
        else
        {
            return Ok(KeycloakUserExtensions.ToFieldDict(user, fields));
        }
    }

    /// <summary>
    /// Изменение информации о пользователе по ID
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserById(
        string id, [FromBody] UpdateUserRequest data, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Id parameter is required.");
        }

        if (!Guid.TryParseExact(id, "D", out _))
        {
            return BadRequest("Id must be a valid GUID in format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
        }
        data.Id = id;

        var response = await _keycloakService.UpdateUser(data, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Изменение информации о контактах пользователя по ID
    /// </summary>
    [HttpPut("{id}/contacts")]
    public async Task<IActionResult> GetUserContactsById(
        string id,CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Id parameter is required.");
        }

        if (!Guid.TryParseExact(id, "D", out _))
        {
            return BadRequest("Id must be a valid GUID in format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
        }

        var response = await _keycloakService.GetUserInfo(id, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Получение ФИО пользователей по списку ID
    /// </summary>
    [HttpGet("fullnames")]
    public async Task<IActionResult> GetUsersFullNames(
        [FromQuery] List<string> ids,
        CancellationToken cancellationToken)
    {
        if (ids == null || ids.Count == 0)
        {
            return BadRequest("At least one id is required.");
        }

        var results = new List<object>();

        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParseExact(id, "D", out _))
            {
                results.Add(new { Id = id, Error = "Invalid GUID format", FullName = (string?)null });
                continue;
            }

            var user = await _keycloakService.GetUserInfo(id, cancellationToken);
            if (user == null)
            {
                results.Add(new { Id = id, Error = "User not found", FullName = (string?)null });
                continue;
            }

            var profile = user.ToUserProfile();
            results.Add(new { Id = id, FirstName = profile.FirstName, SecondName = profile.SecondName, LastName = profile.LastName });
        }

        return Ok(results);
    }

    /// <summary>
    /// Получение списка доступных полей
    /// </summary>
    [HttpGet("fields")]
    public async Task<IActionResult> GetUsersField(CancellationToken cancellationToken)
    {
        UserFields v = await _keycloakService.GetUserProfileSchemaAsync(cancellationToken).ConfigureAwait(false);
        return Ok(v.Attributes.ConvertAll(x => x.Name).ToArray());
    }

    /// <summary>
    /// Список пользователей с пагинацией и поиском.
    /// fields управляет составом возвращаемых полей (как в GetUserById).
    /// Если fields пуст — возвращаем базовый набор (id, firstName, secondName,
    /// lastName, username).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListUsers(
        [FromQuery] int offset,
        [FromQuery] int limit,
        [FromQuery] string? search,
        [FromQuery] List<string>? fields,
        CancellationToken cancellationToken)
    {
        if (limit <= 0) limit = 20;
        if (limit > 100) limit = 100;   // защита от слишком больших страниц
        if (offset < 0) offset = 0;

        var (users, total) = await _keycloakService.ListUsersAsync(
            offset, limit, search, cancellationToken);

        // Поля по умолчанию, если клиент не указал fields
        var requestedFields = (fields is { Count: > 0 })
            ? fields
            : new List<string> { "firstName", "secondName", "lastName", "username" };

        var items = users.Select(u =>
        {
            // ToFieldDict отдаёт Dictionary<string,string>; добавляем id отдельно,
            // т.к. он нужен фронту всегда для перехода к профилю/приглашения.
            var dict = u.ToFieldDict(requestedFields) ?? new Dictionary<string, string>();
            var result = new Dictionary<string, string?>(dict.Count + 1)
            {
                ["id"] = u.Id
            };
            foreach (var kv in dict)
                result[kv.Key] = kv.Value;
            return result;
        }).ToList();

        return Ok(new PagedUsersResponse
        {
            Total = total,
            Items = items
        });
    }
}   
