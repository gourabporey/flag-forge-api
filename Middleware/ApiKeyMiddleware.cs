using FlagForge.Data;
using FlagForge.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Middleware;

public class ApiKeyMiddleware(RequestDelegate next)
{
    public const string TenantIdItemKey = "ApiKeyTenantId";
    public const string EnvironmentIdItemKey = "ApiKeyEnvironmentId";
    private const string ApiKeyHeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValues))
        {
            await next(context);
            return;
        }

        var apiKey = apiKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key is required.");
            return;
        }

        var apiKeyHash = EnvironmentApiKeyHasher.Hash(apiKey);
        var environment = await dbContext.Environments
            .AsNoTracking()
            .Where(x => x.ApiKeyHash == apiKeyHash)
            .Select(x => new { x.EnvironmentId, x.TenantId })
            .SingleOrDefaultAsync(context.RequestAborted);

        if (environment is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API key.");
            return;
        }

        context.Items[TenantIdItemKey] = environment.TenantId;
        context.Items[EnvironmentIdItemKey] = environment.EnvironmentId;
        await next(context);
    }
}
