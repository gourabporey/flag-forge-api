using Asp.Versioning;
using FlagForge.Data.Services;
using FlagForge.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FlagForge.Controllers;

[ApiController]
[ApiVersion(1.0)]
[SwaggerTag("Register and login users")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [SwaggerOperation(OperationId = "Auth.Register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct
    )
    {
        try
        {
            var response = await authService.RegisterAsync(request, ct);
            return Created($"/api/v1/users/{response.UserId}", response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(OperationId = "Auth.Login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct
    )
    {
        try
        {
            var response = await authService.LoginAsync(request, ct);
            return response is null ? Unauthorized("Invalid email or password.") : Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
