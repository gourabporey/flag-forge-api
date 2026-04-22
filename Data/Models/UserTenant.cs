namespace FlagForge.Data.Models;

public class UserTenant
{
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }

    public User? User { get; init; }
    public Tenant? Tenant { get; init; }
}
