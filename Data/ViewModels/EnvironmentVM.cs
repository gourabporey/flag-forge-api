using FlagForge.Data.Models;

namespace FlagForge.Data.ViewModels;

public record CreateEnvironmentRequest(Guid TenantId, string Name);

public record EnvironmentResponse(Guid EnvironmentId, Guid TenantId, string Name)
{
    public static EnvironmentResponse FromEnvironment(FeatureFlagEnvironment environment)
    {
        return new EnvironmentResponse(environment.EnvironmentId, environment.TenantId, environment.Name);
    }
}

public record CreateEnvironmentResponse(Guid EnvironmentId, Guid TenantId, string Name, string ApiKey)
{
    public static CreateEnvironmentResponse FromEnvironment(FeatureFlagEnvironment environment, string apiKey)
    {
        return new CreateEnvironmentResponse(environment.EnvironmentId, environment.TenantId, environment.Name, apiKey);
    }
}
