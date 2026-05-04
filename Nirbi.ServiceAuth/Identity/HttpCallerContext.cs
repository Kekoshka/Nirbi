using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Nirbi.ServiceAuth.Identity;

public sealed class HttpCallerContext : ICallerContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCallerContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsService => User.IsNirbiServiceCaller();
}
