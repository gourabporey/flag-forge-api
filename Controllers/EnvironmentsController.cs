namespace FlagForge.Controllers;

using Asp.Versioning;
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
public class EnvironmentsController(EnvironmentService environmentService) : ControllerBase
{
    private const string ValidationFailedMessage = "Validation failed for request";

    [HttpPost]
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
            var environment = await environmentService.CreateEnvironmentAsync(request, ct);
            return Created($"/api/environments/{environment.EnvironmentId}", environment);
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

        var environments = await environmentService.GetEnvironmentsAsync(tenantId, ct);

        return Ok(environments);
    }
}
