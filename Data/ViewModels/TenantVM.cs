using FlagForge.Data.Models;

namespace FlagForge.Data.ViewModels;

public record CreateTenantRequest(string Name, TenantPlan? Plan);

public record TenantResponse(Guid TenantId, string Name, TenantPlan Plan, DateTimeOffset CreatedAt)
{
    public static TenantResponse FromTenant(Tenant tenant)
    {
        return new TenantResponse(tenant.TenantId, tenant.Name, tenant.Plan, tenant.CreatedAt);
    }
}
