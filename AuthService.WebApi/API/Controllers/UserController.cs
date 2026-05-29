using AuthService.WebApi.Domain.Services;
using AuthService.WebApi.External.Keycloak;
using AuthService.WebApi.External.Keycloak.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.WebApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IKeycloakIntegrationService _keycloakService;

    public UsersController(IKeycloakIntegrationService keycloakService)
        => _keycloakService = keycloakService;

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
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(
        string id,
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

        var response = KeycloakUserExtensions.ToUserProfile(user);

        return Ok(response);
    }

    /// <summary>
    /// Получение ФИО пользователей по списку ID
    /// </summary>
    [HttpGet("users/fullnames")]
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
}   
