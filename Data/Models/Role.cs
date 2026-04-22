namespace FlagForge.Data.Models;

public class Role
{
    public int RoleId { get; init; }
    public string Name { get; init; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; init; } = new List<UserRole>();
}
