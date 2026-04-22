using FlagForge.Data.Models;
using FlagForge.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data.Services;

public class TenantService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<TenantResponse> CreateTenantAsync(
        CreateTenantRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        var exists = await _context.Tenants
            .AsNoTracking()
            .AnyAsync(x => x.Name == name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("A tenant with that name already exists.");
        }

        var tenant = new Tenant
        {
            Name = name,
            Plan = request.Plan ?? TenantPlan.Tier1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Tenants.Add(tenant);
        _context.UserTenants.Add(new UserTenant { UserId = userId, Tenant = tenant });
        await _context.SaveChangesAsync(cancellationToken);

        return TenantResponse.FromTenant(tenant);
    }

    public async Task<IReadOnlyList<TenantResponse>> GetTenantsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Tenant!.Name)
            .Select(x => TenantResponse.FromTenant(x.Tenant!))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasTenantAccessAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.TenantId == tenantId, cancellationToken);
    }
}
