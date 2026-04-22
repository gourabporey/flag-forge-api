namespace FlagForge.Controllers;

using Asp.Versioning;
using Data.Models;
using Data.Services;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[SwaggerTag("Create and Get Feature flags")]
[Route("api/v{version:apiVersion}/feature-flags")]
public class FeatureFlagController(FeatureFlagService featureFlagService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(OperationId = "FeatureFlags.CreateFeatureFlag")]
    [ProducesResponseType(typeof(FeatureFlag), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FeatureFlag>> CreateFeatureFlag(
        [FromBody] FeatureFlagVM featureFlag,
        CancellationToken ct
    )
    {
        var flag = await featureFlagService.AddFeatureFlagAsync(featureFlag, ct);
        return Created($"api/v1/feature-flags/{flag.FlagId}", flag);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "FeatureFlags.GetAllFeatureFlags")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureFlag>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<FeatureFlag>>> GetFeatureFlags(CancellationToken ct)
    {
        return Ok(await featureFlagService.GetAllFeatureFlagsAsync(ct));
    }
}
