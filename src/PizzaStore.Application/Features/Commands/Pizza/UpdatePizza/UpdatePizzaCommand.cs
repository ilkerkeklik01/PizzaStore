using MediatR;

namespace PizzaStore.Application.Features.Commands.Pizza.UpdatePizza;

public record UpdatePizzaCommand(string Id, UpdatePizzaDto UpdatePizzaDto) : IRequest<UpdatePizzaResponse>;
