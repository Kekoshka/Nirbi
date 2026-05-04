using ExceptionHandler.Exceptions;
using MinorTaskService.WebApi.Interfaces;
using System.Security.Claims;

namespace MinorTaskService.WebApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor) 
        {
        _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var idClaim = user.FindFirst("sub")?.Value
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
                throw new UnauthorizedException("User id claim (sub) is missing or not a GUID.");

            return userId;
        }
    }
}
