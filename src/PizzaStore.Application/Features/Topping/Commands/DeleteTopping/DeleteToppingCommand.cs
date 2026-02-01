using MediatR;

namespace PizzaStore.Application.Features.Topping.Commands.DeleteTopping;

public record DeleteToppingCommand(string Id) : IRequest<DeleteToppingResponse>;
