using FlagForge.Data.Models;
using FlagForge.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data.Services;

public class TenantService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
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
        await _context.SaveChangesAsync(cancellationToken);

        return TenantResponse.FromTenant(tenant);
    }

    public async Task<IReadOnlyList<TenantResponse>> GetTenantsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => TenantResponse.FromTenant(x))
            .ToListAsync(cancellationToken);
    }
}
