using MediatR;

namespace PizzaStore.Application.Features.Commands.Topping.DeleteTopping;

public record DeleteToppingCommand(string Id) : IRequest<DeleteToppingResponse>;
