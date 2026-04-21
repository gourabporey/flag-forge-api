namespace FlagForge.Data.Models;

public class Tenant
{
    public Guid TenantId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public TenantPlan Plan { get; set; } = TenantPlan.Tier1;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<FeatureFlagEnvironment> Environments { get; set; } = new List<FeatureFlagEnvironment>();
}
