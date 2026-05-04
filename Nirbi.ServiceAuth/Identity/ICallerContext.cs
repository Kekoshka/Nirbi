using System.Security.Claims;

namespace Nirbi.ServiceAuth.Identity;

public interface ICallerContext
{
    ClaimsPrincipal? User { get; }
    bool IsService { get; }
}
