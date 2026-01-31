using MediatR;
using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.Application.Features.Commands.Auth.Register;

public record RegisterUserCommand(RegisterUserDto RegisterDto) : IRequest<AuthResponseDto>;
