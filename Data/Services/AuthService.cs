using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FlagForge.Auth;
using FlagForge.Data.Models;
using FlagForge.Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlagForge.Data.Services;

public class AuthService(AppDbContext context, IOptions<JwtOptions> jwtOptions, ILogger<AuthService> logger)
{
    private const int AdminRoleId = 1;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        var tenantName = request.TenantName?.Trim();
        var createdAt = DateTimeOffset.UtcNow;

        var tenant = new Tenant
        {
            Name = CreateUniqueTenantNameAsync(email, tenantName),
            Plan = TenantPlan.Tier1,
            CreatedAt = createdAt
        };
        
        var passwordHash = await Task.Run(() =>
            BCrypt.Net.BCrypt.HashPassword(request.Password), ct);
        
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            CreatedAt = createdAt,
            UserRoles = new List<UserRole> { new() { RoleId = AdminRoleId } },
            UserTenants = new List<UserTenant> { new() { Tenant = tenant } }
        };
        
        context.Users.Add(user);
        
        await context.SaveChangesAsync(ct);

        return new RegisterResponse(user.UserId, user.Email);
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await context.Users
            .AsNoTracking()
            .Where(x => x.Email == email && x.IsActive)
            .Select(x => new
            {
                x.UserId,
                x.Email,
                x.PasswordHash
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var roles = await context.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == user.UserId)
            .OrderBy(x => x.Role!.Name)
            .Select(x => x.Role!.Name)
            .ToListAsync(cancellationToken);

        var tenants = await context.UserTenants
            .AsNoTracking()
            .Where(x => x.UserId == user.UserId)
            .OrderBy(x => x.Tenant!.Name)
            .Select(x => new AuthTenantResponse(x.TenantId, x.Tenant!.Name))
            .ToListAsync(cancellationToken);

        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("User is not assigned to any tenant.");
        }

        var selectedTenant = request.TenantId.HasValue
            ? tenants.SingleOrDefault(x => x.TenantId == request.TenantId.Value)
            : tenants.First();

        if (selectedTenant is null)
        {
            throw new UnauthorizedAccessException("User does not have access to the selected tenant.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var accessToken = GenerateAccessToken(user.UserId, user.Email, selectedTenant.TenantId, roles, expiresAt);
        var refreshTokenValue = GenerateRefreshToken();

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshTokenValue,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshTokenValue,
            expiresAt,
            user.UserId,
            user.Email,
            selectedTenant.TenantId,
            roles,
            tenants);
    }

    private string GenerateAccessToken(
        Guid userId,
        string email,
        Guid tenantId,
        IReadOnlyCollection<string> roles,
        DateTimeOffset expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new("tenantId", tenantId.ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateUniqueTenantNameAsync(string email, string? requestedTenant)
    {
        if (!string.IsNullOrWhiteSpace(requestedTenant)) return requestedTenant;
        var localPart = email[..email.IndexOf('@')];
        var tenantName = $"{localPart}'s workspace {Guid.NewGuid().ToString("N")[..6]}";
        return tenantName;
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
