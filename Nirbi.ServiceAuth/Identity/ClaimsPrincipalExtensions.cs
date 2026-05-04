using System.Security.Claims;

namespace Nirbi.ServiceAuth.Identity;

public static class ClaimsPrincipalExtensions
{
    public static bool IsNirbiServiceCaller(this ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        var type = principal.FindFirst(ServiceAuthClaimTypes.CallerType)?.Value;
        return string.Equals(type, ServiceAuthClaimTypes.CallerTypeService, StringComparison.Ordinal);
    }

    public static bool IsNirbiUserCaller(this ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        var type = principal.FindFirst(ServiceAuthClaimTypes.CallerType)?.Value;
        return string.Equals(type, ServiceAuthClaimTypes.CallerTypeUser, StringComparison.Ordinal);
    }
}
