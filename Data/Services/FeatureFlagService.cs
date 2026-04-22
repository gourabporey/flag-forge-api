using FlagForge.Data.Models;
using FlagForge.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data.Services;

public class FeatureFlagService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<FeatureFlagResponse> AddFeatureFlagAsync(
        FeatureFlagVM featureFlag,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var environmentId = featureFlag.EnvironmentId
            ?? await GetOrCreateLegacyEnvironmentIdAsync(tenantId, featureFlag.Environment, cancellationToken);

        var environmentBelongsToTenant = await _context.Environments
            .AsNoTracking()
            .AnyAsync(x => x.EnvironmentId == environmentId && x.TenantId == tenantId, cancellationToken);

        if (!environmentBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Environment is not available for the current tenant.");
        }

        var flagName = featureFlag.Name.Trim();
        if (string.IsNullOrWhiteSpace(flagName))
        {
            throw new ArgumentException("Feature flag name is required.");
        }

        var flagExists = await _context.FeatureFlags
            .AsNoTracking()
            .AnyAsync(x => x.EnvironmentId == environmentId && x.Name == flagName, cancellationToken);

        if (flagExists)
        {
            throw new InvalidOperationException("A feature flag with that name already exists for this environment.");
        }

        var flag = new FeatureFlag
        {
            EnvironmentId = environmentId,
            Name = flagName,
            Enabled = featureFlag.IsEnabled,
            Rules = string.IsNullOrWhiteSpace(featureFlag.Rules) ? "{}" : featureFlag.Rules,
            Version = 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.FeatureFlags.Add(flag);
        await _context.SaveChangesAsync(cancellationToken);

        return FeatureFlagResponse.FromFeatureFlag(flag);
    }

    public async Task<IReadOnlyList<FeatureFlagResponse>> GetAllFeatureFlagsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .AsNoTracking()
            .Where(x => x.Environment!.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .Select(x => new FeatureFlagResponse(
                x.FlagId,
                x.EnvironmentId,
                x.Name,
                x.Enabled,
                x.Rules,
                x.Version,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    private async Task<Guid> GetOrCreateLegacyEnvironmentIdAsync(
        Guid tenantId,
        string? environmentName,
        CancellationToken cancellationToken)
    {
        var normalizedEnvironmentName = string.IsNullOrWhiteSpace(environmentName)
            ? "dev"
            : environmentName.Trim().ToLowerInvariant();

        var existingEnvironment = await _context.Environments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Name == normalizedEnvironmentName)
            .Select(x => new { x.EnvironmentId })
            .SingleOrDefaultAsync(cancellationToken);

        if (existingEnvironment is not null)
        {
            return existingEnvironment.EnvironmentId;
        }

        var tenant = await _context.Tenants
            .SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (tenant is null)
        {
            throw new KeyNotFoundException("Tenant was not found.");
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
