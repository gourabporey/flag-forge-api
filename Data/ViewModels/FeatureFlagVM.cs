namespace FlagForge.Data.ViewModels;

using FlagForge.Data.Models;

public class FeatureFlagVM
{
    public Guid? EnvironmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string? Environment { get; set; }
    public string Rules { get; set; } = "{}";
    public List<string>? Tags { get; set; }
}

public record FeatureFlagResponse(
    Guid FlagId,
    Guid EnvironmentId,
    string Name,
    bool Enabled,
    string Rules,
    long Version,
    DateTimeOffset UpdatedAt)
{
    public static FeatureFlagResponse FromFeatureFlag(FeatureFlag flag)
    {
        return new FeatureFlagResponse(
            flag.FlagId,
            flag.EnvironmentId,
            flag.Name,
            flag.Enabled,
            flag.Rules,
            flag.Version,
            flag.UpdatedAt);
    }
}
