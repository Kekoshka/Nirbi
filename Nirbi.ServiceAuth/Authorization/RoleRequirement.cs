using Microsoft.AspNetCore.Authorization;

namespace Nirbi.ServiceAuth.Authorization;

public sealed class RoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }
    public RoleRequirement(string role) => Role = role;
}
