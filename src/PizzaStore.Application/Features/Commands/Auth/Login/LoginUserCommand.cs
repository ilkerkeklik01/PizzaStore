using MediatR;
using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.Application.Features.Commands.Auth.Login;

public record LoginUserCommand(LoginUserDto LoginDto) : IRequest<AuthResponseDto>;
