using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Nirbi.ServiceAuth.Authorization;

public sealed class ServiceAuthPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public ServiceAuthPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("NirbiScope:", StringComparison.Ordinal))
        {
            var scope = policyName["NirbiScope:".Length..];
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ScopeRequirement(scope))
                .Build();
        }

        if (policyName.StartsWith("NirbiRole:", StringComparison.Ordinal))
        {
            var role = policyName["NirbiRole:".Length..];
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new RoleRequirement(role))
                .Build();
        }

        return await base.GetPolicyAsync(policyName).ConfigureAwait(false);
    }
}
