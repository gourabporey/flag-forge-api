using FlagForge.Data.Models;
using FlagForge.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data.Services;

public class FeatureFlagService(AppDbContext context)
{
    private const string DefaultTenantName = "Default";
    private readonly AppDbContext _context = context;

    public async Task<FeatureFlag> AddFeatureFlagAsync(FeatureFlagVM featureFlag, CancellationToken cancellationToken = default)
    {
        var environmentId = featureFlag.EnvironmentId
            ?? await GetOrCreateLegacyEnvironmentIdAsync(featureFlag.Environment, cancellationToken);

        var flag = new FeatureFlag
        {
            EnvironmentId = environmentId,
            Name = featureFlag.Name,
            Enabled = featureFlag.IsEnabled,
            Rules = string.IsNullOrWhiteSpace(featureFlag.Rules) ? "{}" : featureFlag.Rules,
            Version = 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.FeatureFlags.Add(flag);
        await _context.SaveChangesAsync(cancellationToken);

        return flag;
    }

    public async Task<IReadOnlyList<FeatureFlag>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    private async Task<Guid> GetOrCreateLegacyEnvironmentIdAsync(string? environmentName, CancellationToken cancellationToken)
    {
        var normalizedEnvironmentName = string.IsNullOrWhiteSpace(environmentName)
            ? "dev"
            : environmentName.Trim().ToLowerInvariant();

        var existingEnvironment = await _context.Environments
            .AsNoTracking()
            .Where(x => x.Tenant!.Name == DefaultTenantName && x.Name == normalizedEnvironmentName)
            .Select(x => new { x.EnvironmentId })
            .SingleOrDefaultAsync(cancellationToken);

        if (existingEnvironment is not null)
        {
            return existingEnvironment.EnvironmentId;
        }

        var tenant = await _context.Tenants
            .SingleOrDefaultAsync(x => x.Name == DefaultTenantName, cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = DefaultTenantName,
                Plan = TenantPlan.Tier1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Tenants.Add(tenant);
        }

        var environment = new FeatureFlagEnvironment
        {
            Tenant = tenant,
            Name = normalizedEnvironmentName,
            ApiKeyHash = EnvironmentApiKeyHasher.Hash($"local-{tenant.TenantId:N}-{normalizedEnvironmentName}")
        };

        _context.Environments.Add(environment);
        await _context.SaveChangesAsync(cancellationToken);

        return environment.EnvironmentId;
    }
}
