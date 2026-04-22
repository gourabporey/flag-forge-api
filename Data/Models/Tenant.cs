namespace FlagForge.Data.Models;

public class Tenant
{
    public Guid TenantId { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public TenantPlan Plan { get; init; } = TenantPlan.Tier1;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public ICollection<FeatureFlagEnvironment> Environments { get; init; } = new List<FeatureFlagEnvironment>();
    public ICollection<UserTenant> UserTenants { get; init; } = new List<UserTenant>();
}
