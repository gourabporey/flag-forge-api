using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FlagForge.Auth;
using FlagForge.Data.Caching.Interfaces;
using FlagForge.Data.Models;
using FlagForge.Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlagForge.Data.Services;

public class AuthService(
    AppDbContext context,
    IOptions<JwtOptions> jwtOptions,
    IAuthCache authCache,
    ILogger<AuthService> logger
)
{
    private const int AdminRoleId = 1;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken ct = default
    )
    {
        var email = NormalizeEmail(request.Email);
        var tenantName = request.TenantName?.Trim();
        var createdAt = DateTimeOffset.UtcNow;

        var tenant = new Tenant
        {
            Name = CreateUniqueTenantNameAsync(email, tenantName),
            Plan = TenantPlan.Tier1,
            CreatedAt = createdAt,
        };

        var passwordHash = await Task.Run(
            () => BCrypt.Net.BCrypt.HashPassword(request.Password),
            ct
        );

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            CreatedAt = createdAt,
            UserRoles = new List<UserRole> { new() { RoleId = AdminRoleId } },
            UserTenants = new List<UserTenant> { new() { Tenant = tenant } },
        };

        context.Users.Add(user);

        await context.SaveChangesAsync(ct);

        return new RegisterResponse(user.UserId, user.Email);
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default
    )
    {
        var email = NormalizeEmail(request.Email);
        var userSnapshot = await authCache.GetAsync(email, ct);

        if (userSnapshot is null)
        {
            logger.LogInformation("Cache miss for {Email}", request.Email);
            
            var user = await context
                .Users.AsNoTracking()
                .Where(x => x.Email == email && x.IsActive)
                .Select(x => new
                {
                    x.UserId,
                    x.PasswordHash,
                    x.Email,
                    Roles = x.UserRoles.Select(ur => ur.Role!.Name).ToList(),
                    Tenants = x
                        .UserTenants.Select(ut => new AuthTenantResponse(
                            ut.TenantId,
                            ut.Tenant!.Name
                        ))
                        .ToList(),
                })
                .FirstOrDefaultAsync(ct);

            if (user is null)
                return null;

            if (user.Tenants.Count == 0)
            {
                throw new InvalidOperationException("User is not assigned to any tenant.");
            }

            userSnapshot = new AuthSnapshot
            {
                UserId = user.UserId,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                IsActive = true,
                Roles = user.Roles,
                Tenants = user.Tenants,
            };

            await authCache.SetAsync(userSnapshot, TimeSpan.FromMinutes(5), ct);
        }
        else
        {
            logger.LogInformation("Cache hit for {Email}", request.Email);
        }

        var isPasswordValid = await Task.Run(
            () => BCrypt.Net.BCrypt.Verify(request.Password, userSnapshot.PasswordHash),
            ct
        );
        if (!isPasswordValid)
            return null;

        var selectedTenant = request.TenantId.HasValue
            ? userSnapshot.Tenants.FirstOrDefault(x => x.TenantId == request.TenantId.Value)
            : userSnapshot.Tenants[0];
        if (selectedTenant is null)
        {
            throw new UnauthorizedAccessException(
                "User does not have access to the selected tenant."
            );
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var accessToken = GenerateAccessToken(
            userSnapshot.UserId,
            userSnapshot.Email,
            selectedTenant.TenantId,
            userSnapshot.Roles,
            expiresAt
        );
        var refreshTokenValue = GenerateRefreshToken();

        context.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = userSnapshot.UserId,
                Token = refreshTokenValue,
                ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays),
                IsRevoked = false,
                CreatedAt = now,
            }
        );

        await context.SaveChangesAsync(ct);

        return new LoginResponse(
            accessToken,
            refreshTokenValue,
            expiresAt,
            userSnapshot.UserId,
            userSnapshot.Email,
            selectedTenant.TenantId,
            userSnapshot.Roles,
            userSnapshot.Tenants
        );
    }

    private string GenerateAccessToken(
        Guid userId,
        string email,
        Guid tenantId,
        IReadOnlyCollection<string> roles,
        DateTimeOffset expiresAt
    )
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new("tenantId", tenantId.ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateUniqueTenantNameAsync(string email, string? requestedTenant)
    {
        if (!string.IsNullOrWhiteSpace(requestedTenant))
            return requestedTenant;
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
