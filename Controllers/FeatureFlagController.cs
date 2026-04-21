namespace FlagForge.Controllers;

using FlagForge.Data.Models;
using FlagForge.Data.Services;
using FlagForge.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/feature-flags")]
public class FeatureFlagController(FeatureFlagService featureFlagService) : ControllerBase
{
    private readonly FeatureFlagService _featureFlagService = featureFlagService;

    [HttpPost]
    public async Task<FeatureFlag> PostFeatureFlag([FromBody] FeatureFlagVM featureFlag, CancellationToken cancellationToken)
    {
        return await _featureFlagService.AddFeatureFlagAsync(featureFlag, cancellationToken);
    }

    [HttpGet]
    public async Task<IReadOnlyList<FeatureFlag>> GetFeatureFlags(CancellationToken cancellationToken)
    {
        return await _featureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
    }
}
