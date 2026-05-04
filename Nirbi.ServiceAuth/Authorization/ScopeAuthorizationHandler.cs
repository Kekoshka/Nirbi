using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Nirbi.ServiceAuth.Authorization;

public sealed class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        var scopes = GetScopes(context.User);
        if (scopes.Contains(requirement.Scope, StringComparer.Ordinal))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }

    private static HashSet<string> GetScopes(ClaimsPrincipal user)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var claim in user.FindAll("scope"))
        {
            foreach (var part in claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                set.Add(part);
        }

        return set;
    }
}
