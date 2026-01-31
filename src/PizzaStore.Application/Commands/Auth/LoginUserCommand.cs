using MediatR;
using PizzaStore.Application.DTOs;

namespace PizzaStore.Application.Commands.Auth;

public record LoginUserCommand(LoginUserDto LoginDto) : IRequest<AuthResponseDto>;
