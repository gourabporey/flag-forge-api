namespace FlagForge.Data.ViewModels;

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
