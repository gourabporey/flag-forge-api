using System.Text.Json;
using FlagForge.Data.Caching.Interfaces;
using FlagForge.Data.Models;
using StackExchange.Redis;

namespace FlagForge.Data.Caching;

public sealed class RedisAuthCache(IConnectionMultiplexer redis) : IAuthCache
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static string Key(string email) => $"auth:user:{email}";

    public async Task<AuthSnapshot?> GetAsync(string email, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(Key(email));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<AuthSnapshot>(value!, JsonOptions);
    }

    public async Task SetAsync(AuthSnapshot snapshot, TimeSpan ttl, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);

        await _db.StringSetAsync(
            Key(snapshot.Email),
            json,
            expiry: ttl);
    }

    public async Task RemoveAsync(string email, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(Key(email));
    }
}