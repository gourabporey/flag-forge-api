namespace FlagForge.Data.Models;

public class FeatureFlagEnvironment
{
    public Guid EnvironmentId { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ApiKeyHash { get; init; } = string.Empty;

    public Tenant? Tenant { get; init; }
    public ICollection<FeatureFlag> FeatureFlags { get; init; } = new List<FeatureFlag>();
}
