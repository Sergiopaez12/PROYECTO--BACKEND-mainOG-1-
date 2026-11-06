using ECommerce.Application.Commands.Login;
using ECommerce.Application.Commands.Register;
using ECommerce.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Command — registro de usuario (público).</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthDto>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(
                new RegisterCommand(request.Email, request.Name, request.Password),
                ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Command — login (devuelve JWT).</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthDto>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(
                new LoginCommand(request.Email, request.Password),
                ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}

public record RegisterRequest(string Email, string Name, string Password);

public record LoginRequest(string Email, string Password);
