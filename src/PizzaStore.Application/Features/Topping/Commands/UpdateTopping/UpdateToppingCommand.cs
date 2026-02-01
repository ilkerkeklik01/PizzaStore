using MediatR;

namespace PizzaStore.Application.Features.Topping.Commands.UpdateTopping;

public record UpdateToppingCommand(string Id, UpdateToppingDto UpdateToppingDto) : IRequest<UpdateToppingResponse>;
