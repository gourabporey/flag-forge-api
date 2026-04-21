namespace FlagForge.Data.Models;

public class FeatureFlag
{
    public Guid FlagId { get; set; } = Guid.NewGuid();
    public Guid EnvironmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Rules { get; set; } = "{}";
    public long Version { get; set; } = 1;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public FeatureFlagEnvironment? Environment { get; set; }
}
