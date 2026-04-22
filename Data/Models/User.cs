namespace FlagForge.Data.Models;

public class User
{
    public Guid UserId { get; init; } = Guid.NewGuid();
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public ICollection<UserRole> UserRoles { get; init; } = new List<UserRole>();
    public ICollection<UserTenant> UserTenants { get; init; } = new List<UserTenant>();
    public ICollection<RefreshToken> RefreshTokens { get; init; } = new List<RefreshToken>();
}
