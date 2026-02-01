using MediatR;

namespace PizzaStore.Application.Features.Pizza.Commands.CreatePizza;

public record CreatePizzaCommand(CreatePizzaDto CreatePizzaDto) : IRequest<CreatePizzaResponse>;
