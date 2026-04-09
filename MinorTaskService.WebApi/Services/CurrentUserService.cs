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
            if (user is null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var userIdString = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString is null || Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedException("Claim \"nameidentifier\" is empty");
            
            return userId;
        }
    }
}
