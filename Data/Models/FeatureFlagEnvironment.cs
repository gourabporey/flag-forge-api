namespace FlagForge.Data.Models;

public class FeatureFlagEnvironment
{
    public Guid EnvironmentId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public Tenant? Tenant { get; set; }
    public ICollection<FeatureFlag> FeatureFlags { get; set; } = new List<FeatureFlag>();
}
