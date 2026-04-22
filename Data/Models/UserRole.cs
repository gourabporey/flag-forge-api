namespace FlagForge.Data.Models;

public class UserRole
{
    public Guid UserId { get; init; }
    public int RoleId { get; init; }

    public User? User { get; init; }
    public Role? Role { get; init; }
}
