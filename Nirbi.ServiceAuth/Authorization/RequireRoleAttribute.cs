using Microsoft.AspNetCore.Authorization;

namespace Nirbi.ServiceAuth.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(string role) => Policy = $"NirbiRole:{role}";
}
