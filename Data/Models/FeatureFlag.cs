namespace FlagForge.Data.Models;

public class FeatureFlag
{
    public Guid FlagId { get; init; } = Guid.NewGuid();
    public Guid EnvironmentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public string Rules { get; init; } = "{}";
    public long Version { get; init; } = 1;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    public FeatureFlagEnvironment? Environment { get; init; }
}
