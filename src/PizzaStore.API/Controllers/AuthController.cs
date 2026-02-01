using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaStore.Application.Features.Commands.Auth.Register;
using PizzaStore.Application.Features.Commands.Auth.Login;
using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registerDto">User registration information including firstName, lastName, email, and password</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Returns the JWT token and user details</response>
    /// <response code="400">If the registration data is invalid or email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        var command = new RegisterUserCommand(registerDto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="loginDto">User credentials (email and password)</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Returns the JWT token and user details</response>
    /// <response code="401">If the credentials are invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
    {
        var command = new LoginUserCommand(loginDto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user's ID, email, and roles</returns>
    /// <response code="200">Returns the authenticated user's information</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            UserId = userId,
            Email = email,
            Roles = roles
        });
    }
}
