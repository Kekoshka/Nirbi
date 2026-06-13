using AuthService.WebApi.API.Requests;
using AuthService.WebApi.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.WebApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTokenController : ControllerBase
    {
        private readonly IServiceTokenService _serviceTokenService;

        public ServiceTokenController(IServiceTokenService serviceTokenService)
        {
            _serviceTokenService = serviceTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterService(
            [FromBody] RegisterServiceRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _serviceTokenService.RegisterServiceAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetServiceToken(
            [FromBody] ServiceTokenRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _serviceTokenService.IssueServiceTokenAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{serviceId}")]
        public async Task<IActionResult> GetService(
            string serviceId,
            CancellationToken cancellationToken)
        {
            var response = await _serviceTokenService.GetServiceAsync(serviceId, cancellationToken);

            if (response == null)
                return NotFound();

            return Ok(response);
        }

        [HttpPut("{serviceId}/scopes")]
        public async Task<IActionResult> UpdateScopes(
            string serviceId,
            [FromBody] UpdateScopesRequest request,
            CancellationToken cancellationToken)
        {
            await _serviceTokenService.UpdateServiceScopesAsync(
                serviceId,
                request.Scopes,
                cancellationToken
            );

            return Ok();
        }

        [HttpPost("{serviceId}/revoke")]
        public async Task<IActionResult> RevokeService(
            string serviceId,
            CancellationToken cancellationToken)
        {
            await _serviceTokenService.RevokeServiceAsync(serviceId, cancellationToken);
            return Ok();
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> HealthGet()
        {
            return Ok(new { status = "healthy" });
        }
    }
}
