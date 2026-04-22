using Asp.Versioning;
using FlagForge.Auth;
using FlagForge.Data.Services;
using FlagForge.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FlagForge.Controllers;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[SwaggerTag("Create and Get Tenants")]
[Route("api/v{version:apiVersion}/tenants")]
public class TenantsController(TenantService tenantService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(OperationId = "Tenants.CreateTenant")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantResponse>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Tenant name is required.");
        }

        try
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var tenant = await tenantService.CreateTenantAsync(request, userId, ct);
            return Created($"/tenants/{tenant.TenantId}", tenant);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Developer,Viewer")]
    [SwaggerOperation(OperationId = "Tenants.GetTenants")]
    [ProducesResponseType(typeof(IReadOnlyList<TenantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<TenantResponse>>> GetTenants(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        return Ok(await tenantService.GetTenantsAsync(userId, ct));
    }
}
