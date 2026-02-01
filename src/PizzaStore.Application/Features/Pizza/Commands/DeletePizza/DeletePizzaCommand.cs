using MediatR;

namespace PizzaStore.Application.Features.Pizza.Commands.DeletePizza;

public record DeletePizzaCommand(string Id) : IRequest<DeletePizzaResponse>;
