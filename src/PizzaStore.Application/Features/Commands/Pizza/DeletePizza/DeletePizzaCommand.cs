using MediatR;

namespace PizzaStore.Application.Features.Commands.Pizza.DeletePizza;

public record DeletePizzaCommand(string Id) : IRequest<DeletePizzaResponse>;
