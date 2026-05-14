using AuthService.WebApi.API.Requests;
using AuthService.WebApi.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.WebApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IKeycloakIntegrationService _keycloakService;

    public AuthController(IKeycloakIntegrationService keycloakService)
        => _keycloakService = keycloakService;

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _keycloakService.LoginAsync(
            request.Username, request.Password, cancellationToken);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _keycloakService.RegisterAsync(
            request.Username, request.Email, request.Password, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _keycloakService.RefreshTokenAsync(
            request.RefreshToken, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await _keycloakService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Ok();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var message = await _keycloakService.RequestPasswordResetAsync(
            request.Email, cancellationToken);
        return Ok(new { message });
    }
}