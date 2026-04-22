namespace FlagForge.Data.Models;

public class UsageAuditLog
{
    public Guid LogId { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid EnvironmentId { get; init; }
    public string FlagName { get; init; } = string.Empty;
    public bool EvaluationResult { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public Tenant? Tenant { get; init; }
    public FeatureFlagEnvironment? Environment { get; init; }
}
