using Microsoft.AspNetCore.Authorization;

namespace Nirbi.ServiceAuth.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireScopeAttribute : AuthorizeAttribute
{
    public RequireScopeAttribute(string scope) => Policy = $"NirbiScope:{scope}";
}
