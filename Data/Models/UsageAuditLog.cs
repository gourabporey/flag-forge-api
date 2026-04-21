namespace FlagForge.Data.Models;

public class UsageAuditLog
{
    public Guid LogId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid EnvironmentId { get; set; }
    public string FlagName { get; set; } = string.Empty;
    public bool EvaluationResult { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public Tenant? Tenant { get; set; }
    public FeatureFlagEnvironment? Environment { get; set; }
}
