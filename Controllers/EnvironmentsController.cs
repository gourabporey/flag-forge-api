namespace FlagForge.Controllers;

using Asp.Versioning;
using FlagForge.Auth;
using Data.Services;
using Data.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[SwaggerTag("Create and Get Environments")]
[Route("api/v{version:apiVersion}/environments")]
public class EnvironmentsController(
    EnvironmentService environmentService,
    TenantService tenantService) : ControllerBase
{
    private const string ValidationFailedMessage = "Validation failed for request";

    [HttpPost]
    [Authorize(Roles = "Admin,Developer")]
    [SwaggerOperation(OperationId = "Environments.CreateEnvironment")]
    [ProducesResponseType(typeof(CreateEnvironmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateEnvironmentResponse>> CreateEnvironment(
        [FromBody] CreateEnvironmentRequest request,
        IValidator<CreateEnvironmentRequest> requestValidator,
        CancellationToken ct
    )
    {
        var validationResult = await requestValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(ValidationFailedMessage, validationResult.Errors);
        }

        try
        {
            var userId = User.GetUserId();
            var tenantId = User.GetTenantId();
            if (userId == Guid.Empty || tenantId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (request.TenantId != tenantId
                || !await tenantService.UserHasTenantAccessAsync(userId, request.TenantId, ct))
            {
                return Forbid();
            }

            var environment = await environmentService.CreateEnvironmentAsync(request, ct);
            return Created($"/api/v1/environments/{environment.EnvironmentId}", environment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("{tenantId:guid}")]
    [Authorize(Roles = "Admin,Developer,Viewer")]
    [SwaggerOperation(OperationId = "Environments.GetAllEnvironments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnvironmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<EnvironmentResponse>>> GetEnvironments(
        [FromRoute] Guid tenantId,
        CancellationToken ct
    )
    {
        if (tenantId == Guid.Empty)
        {
            return BadRequest("tenantId query parameter is required.");
        }

        var userId = User.GetUserId();
        var currentTenantId = User.GetTenantId();
        if (userId == Guid.Empty || currentTenantId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (tenantId != currentTenantId
            || !await tenantService.UserHasTenantAccessAsync(userId, tenantId, ct))
        {
            return Forbid();
        }

        var environments = await environmentService.GetEnvironmentsAsync(tenantId, ct);

        return Ok(environments);
    }
}
