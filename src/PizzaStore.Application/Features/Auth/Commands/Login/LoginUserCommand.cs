using MediatR;
using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.Application.Features.Auth.Commands.Login;

public record LoginUserCommand(LoginUserDto LoginDto) : IRequest<AuthResponseDto>;
