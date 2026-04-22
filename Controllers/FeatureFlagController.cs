namespace FlagForge.Controllers;

using Asp.Versioning;
using FlagForge.Auth;
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
    [Authorize(Roles = "Admin,Developer")]
    [SwaggerOperation(OperationId = "FeatureFlags.CreateFeatureFlag")]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FeatureFlagResponse>> CreateFeatureFlag(
        [FromBody] FeatureFlagVM featureFlag,
        CancellationToken ct
    )
    {
        var tenantId = User.GetTenantId();
        if (tenantId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            var flag = await featureFlagService.AddFeatureFlagAsync(featureFlag, tenantId, ct);
            return Created($"api/v1/feature-flags/{flag.FlagId}", flag);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Developer,Viewer")]
    [SwaggerOperation(OperationId = "FeatureFlags.GetAllFeatureFlags")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureFlagResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<FeatureFlagResponse>>> GetFeatureFlags(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId == Guid.Empty)
        {
            return Unauthorized();
        }

        return Ok(await featureFlagService.GetAllFeatureFlagsAsync(tenantId, ct));
    }
}
