using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Core.Auth.Interfaces;
using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.Application.Features.Commands.Auth.Register;

/// <summary>
/// Handles new user registration with role assignment
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(IAuthService authService, ILogger<RegisterUserCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt for user: {Email}", request.RegisterDto.Email);
        
        try
        {
            var response = await _authService.RegisterAsync(request.RegisterDto);
            _logger.LogInformation("User {Email} registered successfully", request.RegisterDto.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed registration attempt for user: {Email}", request.RegisterDto.Email);
            throw;
        }
    }
}
