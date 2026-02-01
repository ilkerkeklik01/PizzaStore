using MediatR;

namespace PizzaStore.Application.Features.Commands.Topping.UpdateTopping;

public record UpdateToppingCommand(string Id, UpdateToppingDto UpdateToppingDto) : IRequest<UpdateToppingResponse>;
