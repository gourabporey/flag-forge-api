using FlagForge.Data.Persistence.Interfaces;
using FlagForge.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDbExceptionTranslator dbExceptionTranslator
) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<FeatureFlagEnvironment> Environments => Set<FeatureFlagEnvironment>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<UsageAuditLog> UsageAuditLogs => Set<UsageAuditLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await base.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw dbExceptionTranslator.Translate(ex);
        }
    }
}
