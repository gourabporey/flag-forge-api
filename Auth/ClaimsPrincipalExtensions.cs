using System.Security.Claims;

namespace FlagForge.Auth;

public static class ClaimsPrincipalExtensions
{
    private const string TenantIdClaim = "tenantId";
    
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    public static Guid GetTenantId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(TenantIdClaim);
        return Guid.TryParse(value, out var tenantId) ? tenantId : Guid.Empty;
    }
}
