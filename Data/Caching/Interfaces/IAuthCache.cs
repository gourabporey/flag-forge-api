using FlagForge.Data.Models;

namespace FlagForge.Data.Caching.Interfaces;

public interface IAuthCache
{
    Task<AuthSnapshot?> GetAsync(string email, CancellationToken ct = default);

    Task SetAsync(AuthSnapshot snapshot, TimeSpan ttl, CancellationToken ct = default);

    Task RemoveAsync(string email, CancellationToken ct = default);
}