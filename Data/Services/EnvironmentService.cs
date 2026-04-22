using System.Security.Cryptography;
using FlagForge.Data.Models;
using FlagForge.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data.Services;

public class EnvironmentService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<CreateEnvironmentResponse> CreateEnvironmentAsync(
        CreateEnvironmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        var tenantExists = await _context.Tenants
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == request.TenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new KeyNotFoundException("Tenant was not found.");
        }

        var environmentExists = await _context.Environments
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == request.TenantId && x.Name == name, cancellationToken);

        if (environmentExists)
        {
            throw new InvalidOperationException("An environment with that name already exists for this tenant.");
        }

        var apiKey = await GenerateUniqueApiKeyAsync(cancellationToken);
        var environment = new FeatureFlagEnvironment
        {
            TenantId = request.TenantId,
            Name = name,
            ApiKeyHash = EnvironmentApiKeyHasher.Hash(apiKey)
        };

        _context.Environments.Add(environment);
        await _context.SaveChangesAsync(cancellationToken);

        return CreateEnvironmentResponse.FromEnvironment(environment, apiKey);
    }

    public async Task<IReadOnlyList<EnvironmentResponse>> GetEnvironmentsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Environments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .Select(x => EnvironmentResponse.FromEnvironment(x))
            .ToListAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueApiKeyAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var apiKey = $"ff_{Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant()}";
            var apiKeyHash = EnvironmentApiKeyHasher.Hash(apiKey);

            var exists = await _context.Environments
                .AsNoTracking()
                .AnyAsync(x => x.ApiKeyHash == apiKeyHash, cancellationToken);

            if (!exists)
            {
                return apiKey;
            }
        }
    }
}
