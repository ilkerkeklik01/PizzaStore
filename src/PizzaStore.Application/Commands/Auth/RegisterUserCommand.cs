using MediatR;
using PizzaStore.Application.DTOs;

namespace PizzaStore.Application.Commands.Auth;

public record RegisterUserCommand(RegisterUserDto RegisterDto) : IRequest<AuthResponseDto>;
