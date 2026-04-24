using FlagForge.Data.ViewModels;

namespace FlagForge.Data.Models;

public sealed class AuthSnapshot
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public string PasswordHash { get; init; } = default!;
    public bool IsActive { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyList<AuthTenantResponse> Tenants { get; init; } = [];
}