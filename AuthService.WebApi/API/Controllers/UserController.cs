using AuthService.WebApi.Domain.Services;
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

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _keycloakService.GetUserByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

}