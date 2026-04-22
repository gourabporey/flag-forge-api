namespace FlagForge.Data.ViewModels;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? TenantName);

public record RegisterResponse(Guid UserId, string Email);

public record LoginRequest(string Email, string Password, Guid? TenantId);

public record AuthTenantResponse(Guid TenantId, string Name);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Email,
    Guid TenantId,
    IReadOnlyList<string> Roles,
    IReadOnlyList<AuthTenantResponse> Tenants);

