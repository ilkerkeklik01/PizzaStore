using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Topping.Commands.CreateTopping;

public record CreateToppingCommand(CreateToppingDto CreateToppingDto) : IRequest<CreateToppingResponse>, IAdminRequest;
