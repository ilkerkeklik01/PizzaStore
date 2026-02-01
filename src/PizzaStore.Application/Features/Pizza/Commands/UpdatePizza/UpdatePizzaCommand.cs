using MediatR;

namespace PizzaStore.Application.Features.Pizza.Commands.UpdatePizza;

public record UpdatePizzaCommand(string Id, UpdatePizzaDto UpdatePizzaDto) : IRequest<UpdatePizzaResponse>;
