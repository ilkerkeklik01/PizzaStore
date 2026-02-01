using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Core.Auth.Interfaces;
using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.Application.Features.Commands.Auth.Login;

/// <summary>
/// Handles user authentication and JWT token generation
/// </summary>
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(IAuthService authService, ILogger<LoginUserCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for user: {Email}", request.LoginDto.Email);
        
        try
        {
            var response = await _authService.LoginAsync(request.LoginDto);
            _logger.LogInformation("User {Email} logged in successfully", request.LoginDto.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed login attempt for user: {Email}", request.LoginDto.Email);
            throw;
        }
    }
}
