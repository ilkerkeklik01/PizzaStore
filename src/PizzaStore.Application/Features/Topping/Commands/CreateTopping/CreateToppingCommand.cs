using MediatR;

namespace PizzaStore.Application.Features.Topping.Commands.CreateTopping;

public record CreateToppingCommand(CreateToppingDto CreateToppingDto) : IRequest<CreateToppingResponse>;
