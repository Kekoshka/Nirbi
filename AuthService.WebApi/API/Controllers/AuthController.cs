using AuthService.WebApi.API.Requests;
using AuthService.WebApi.Domain.Services;
using AuthService.WebApi.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.WebApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IKeycloakIntegrationService _keycloakService;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthController(
            IKeycloakIntegrationService keycloakService,
            JwtTokenGenerator tokenGenerator)
        {
            _keycloakService = keycloakService;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var tokenPair = await _keycloakService.LoginAsync(
                request.Username,
                request.Password,
                cancellationToken
            );

            return Ok(tokenPair);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var tokenPair = await _keycloakService.RegisterAsync(
                request.Username,
                request.Email,
                request.Password,
                cancellationToken
            );

            return Ok(tokenPair);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var tokenPair = await _keycloakService.RefreshTokenAsync(
                request.RefreshToken,
                cancellationToken
            );

            return Ok(tokenPair);
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
                request.Email,
                cancellationToken
            );

            return Ok(new { message });
        }
    }

}
