using MediatR;
using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.Application.Features.Auth.Commands.Register;

public record RegisterUserCommand(RegisterUserDto RegisterDto) : IRequest<AuthResponseDto>;
