using MediatR;
using PizzaStore.Application.Interfaces;
using PizzaStore.Application.DTOs;

namespace PizzaStore.Application.Commands.Auth;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public LoginUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.LoginDto);
    }
}
