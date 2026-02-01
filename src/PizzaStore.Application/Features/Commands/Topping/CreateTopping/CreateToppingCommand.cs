using MediatR;

namespace PizzaStore.Application.Features.Commands.Topping.CreateTopping;

public record CreateToppingCommand(CreateToppingDto CreateToppingDto) : IRequest<CreateToppingResponse>;
