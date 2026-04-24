using Asp.Versioning;
using FlagForge.Data.Exceptions;
using FlagForge.Data.Services;
using FlagForge.Data.ViewModels;
using FlagForge.Extensions;
using FluentValidation;
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
        IValidator<RegisterRequest> requestValidator,
        CancellationToken ct
    )
    {
        var validationResult = await requestValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new RequestValidationException(validationResult.ToErrorsDictionary());
        }

        var response = await authService.RegisterAsync(request, ct);
        return Created($"/api/v1/users/{response.UserId}", response);
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
        var response = await authService.LoginAsync(request, ct);
        return response is null
            ? throw new UnauthorizedAccessException("Invalid email or password.")
            : Ok(response);
    }
}
