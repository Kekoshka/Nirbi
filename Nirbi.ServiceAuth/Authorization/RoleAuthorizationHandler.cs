using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Nirbi.ServiceAuth.Authorization;

public sealed class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        foreach (var role in GetRoles(context.User))
        {
            if (string.Equals(role, requirement.Role, StringComparison.Ordinal))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }

    private static List<string> GetRoles(ClaimsPrincipal user)
    {
        var roles = new List<string>();
        foreach (var claim in user.FindAll(ClaimTypes.Role))
            roles.Add(claim.Value);

        foreach (var claim in user.FindAll("role"))
            roles.Add(claim.Value);

        var realmAccess = user.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
            return roles;

        try
        {
            using var doc = JsonDocument.Parse(realmAccess);
            if (!doc.RootElement.TryGetProperty("roles", out var arr))
                return roles;

            foreach (var r in arr.EnumerateArray())
            {
                var s = r.GetString();
                if (!string.IsNullOrEmpty(s))
                    roles.Add(s);
            }
        }
        catch (JsonException)
        {
        }

        return roles;
    }
}
